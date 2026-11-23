using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Client;

namespace Paragraf.Rag.Payroll;

/// <summary>One compensation component in effect on a date, as comp-ledger stores it: annual amounts.</summary>
public sealed record CompensationComponent(
    [property: JsonPropertyName("component")] string Component,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("effectiveFrom")] DateOnly EffectiveFrom,
    [property: JsonPropertyName("effectiveTo")] DateOnly? EffectiveTo);

public sealed record EmployeeCompensation(
    [property: JsonPropertyName("employeeId")] string EmployeeId,
    [property: JsonPropertyName("asOf")] DateOnly AsOf,
    [property: JsonPropertyName("components")] IReadOnlyList<CompensationComponent> Components)
{
    public const string BaseSalary = "BASE_SALARY";
    public const string Allowance = "ALLOWANCE";

    public CompensationComponent? Find(string code) => Components.FirstOrDefault(c => c.Component == code);
}

public sealed record CompensationHistory(
    [property: JsonPropertyName("employeeId")] string EmployeeId,
    [property: JsonPropertyName("periods")] IReadOnlyList<CompensationComponent> Periods);

/// <summary>The employee's pay data could not be read: unknown employee, no values on the date, or the service failed.</summary>
public sealed class CompensationUnavailableException(string message) : Exception(message);

/// <summary>Where calculations read an employee's pay from.</summary>
public interface ICompensationSource
{
    Task<EmployeeCompensation> GetAsync(string employeeId, DateOnly asOf, CancellationToken ct = default);
    Task<CompensationHistory> GetHistoryAsync(string employeeId, CancellationToken ct = default);
}

public sealed record CompLedgerOptions
{
    /// <summary>The MCP endpoint of a comp-ledger API, for example http://localhost:8080/mcp.</summary>
    public Uri? Endpoint { get; init; }
    public Guid TenantId { get; init; }
}

/// <summary>
/// comp-ledger over MCP: paragraf is the client, and code (not the model) calls two of comp-ledger's read tools.
/// The model never sees comp-ledger's tools, so it cannot reach the write tool (create_snapshot) or another tenant.
/// </summary>
public sealed class CompLedgerClient(CompLedgerOptions options) : ICompensationSource, IAsyncDisposable
{
    private readonly SemaphoreSlim _connect = new(1, 1);
    private McpClient? _client;

    public Task<EmployeeCompensation> GetAsync(string employeeId, DateOnly asOf, CancellationToken ct = default) =>
        CallAsync<EmployeeCompensation>("get_compensation",
            new Dictionary<string, object?> { ["employeeId"] = employeeId, ["asOf"] = asOf.ToString("yyyy-MM-dd") }, ct);

    public Task<CompensationHistory> GetHistoryAsync(string employeeId, CancellationToken ct = default) =>
        CallAsync<CompensationHistory>("get_compensation_history", new Dictionary<string, object?> { ["employeeId"] = employeeId }, ct);

    private async Task<T> CallAsync<T>(string tool, Dictionary<string, object?> arguments, CancellationToken ct)
    {
        var client = await ConnectAsync(ct);
        var result = await client.CallToolAsync(tool, arguments, cancellationToken: ct);
        if (result.StructuredContent is not { } json)
        {
            throw new CompensationUnavailableException($"comp-ledger returned no structured content for {tool}.");
        }
        if (result.IsError == true)
        {
            // A ProblemDetails object: 404 for an unknown employee or no values on the date, 400 for a bad argument.
            var status = json.TryGetProperty("status", out var s) && s.TryGetInt32(out var code) ? code : 0;
            var detail = json.TryGetProperty("detail", out var d) ? d.GetString() : null;
            throw new CompensationUnavailableException(status == 404
                ? $"comp-ledger has no compensation for this employee{(arguments.ContainsKey("asOf") ? " on that date" : "")}."
                : $"comp-ledger refused the request (status {status}): {detail}");
        }
        return json.Deserialize<T>() ?? throw new CompensationUnavailableException($"comp-ledger returned an empty {tool} result.");
    }

    private async Task<McpClient> ConnectAsync(CancellationToken ct)
    {
        if (_client is not null) return _client;
        await _connect.WaitAsync(ct);
        try
        {
            if (_client is not null) return _client;
            var endpoint = options.Endpoint ?? throw new InvalidOperationException("CompLedger:Endpoint is not set.");
            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = endpoint,
                TransportMode = HttpTransportMode.StreamableHttp,
                AdditionalHeaders = new Dictionary<string, string> { ["X-Tenant-Id"] = options.TenantId.ToString() },
                Name = "comp-ledger",
            });
            _client = await McpClient.CreateAsync(transport, new McpClientOptions
            {
                ClientInfo = new() { Name = "paragraf", Version = "1.0" },
            }, cancellationToken: ct);
            return _client;
        }
        finally
        {
            _connect.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null) await _client.DisposeAsync();
        _connect.Dispose();
    }
}
