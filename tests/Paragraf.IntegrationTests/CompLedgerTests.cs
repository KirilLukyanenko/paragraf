using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.AI;
using Npgsql;
using Paragraf.Rag;
using Paragraf.Rag.Payroll;
using Testcontainers.PostgreSql;

namespace Paragraf.IntegrationTests;

/// <summary>
/// The real comp-ledger, built from its Dockerfile, with the demo employees: the MCP client and the calculation tools
/// against it, without a model. Needs a clone of github.com/KirilLukyanenko/comp-ledger at ../comp-ledger or in
/// COMP_LEDGER_DIR; without one the tests are skipped.
/// </summary>
public sealed class CompLedgerFixture : IAsyncLifetime
{
    private INetwork? _network;
    private PostgreSqlContainer? _db;
    private IFutureDockerImage? _image;
    private IContainer? _api;

    public static string? Directory { get; } = FindCompLedger();
    public CompLedgerOptions Options { get; private set; } = new();
    public LawLibrary Library { get; private set; } = null!;

    private static string RepoRoot([System.Runtime.CompilerServices.CallerFilePath] string file = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, "..", ".."));

    private static string? FindCompLedger()
    {
        var dir = Environment.GetEnvironmentVariable("COMP_LEDGER_DIR") ?? Path.Combine(RepoRoot(), "..", "comp-ledger");
        return File.Exists(Path.Combine(dir, "Dockerfile")) ? Path.GetFullPath(dir) : null;
    }

    public async ValueTask InitializeAsync()
    {
        if (Directory is null) return;
        var ct = TestContext.Current.CancellationToken;
        Library = LawLibrary.KodeksPracy(Path.Combine(RepoRoot(), "data"));

        _network = new NetworkBuilder().Build();
        _db = new PostgreSqlBuilder("postgres:17-alpine").WithNetwork(_network).WithNetworkAliases("db")
            .WithPassword(Guid.NewGuid().ToString("N")).Build();
        _image = new ImageFromDockerfileBuilder().WithDockerfileDirectory(Directory).WithDockerfile("Dockerfile").Build();
        await Task.WhenAll(_db.StartAsync(ct), _image.CreateAsync(ct));

        // As comp-ledger's environment requires: a NOLOGIN role and a login in it, before its migrations run.
        var appPassword = Guid.NewGuid().ToString("N");
        await using (var owner = new NpgsqlConnection(_db.GetConnectionString()))
        {
            await owner.OpenAsync(ct);
            await using var command = new NpgsqlCommand(
                $"CREATE ROLE comp_app NOLOGIN; CREATE ROLE comp_app_login LOGIN PASSWORD '{appPassword}' IN ROLE comp_app;", owner);
            await command.ExecuteNonQueryAsync(ct);
        }
        var inNetwork = new NpgsqlConnectionStringBuilder(_db.GetConnectionString()) { Host = "db", Port = 5432 };
        var app = new NpgsqlConnectionStringBuilder(inNetwork.ConnectionString) { Username = "comp_app_login", Password = appPassword };

        _api = new ContainerBuilder(_image)
            .WithNetwork(_network)
            .WithPortBinding(8080, assignRandomHostPort: true)
            .WithEnvironment("ConnectionStrings__App", app.ConnectionString)
            .WithEnvironment("ConnectionStrings__Migrations", inNetwork.ConnectionString)
            .WithEnvironment("Database__MigrateOnStartup", "true")
            .WithEnvironment("TenantAdminApi__Enabled", "true")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/health")))
            .Build();
        await _api.StartAsync(ct);

        var baseUrl = new Uri($"http://{_api.Hostname}:{_api.GetMappedPublicPort(8080)}");
        using var http = new HttpClient { BaseAddress = baseUrl };
        var created = await http.PostAsJsonAsync("/api/v1/tenants", new { name = "paragraf tests" }, ct);
        created.EnsureSuccessStatusCode();
        var tenant = (await created.Content.ReadFromJsonAsync<JsonElement>(ct)).GetProperty("id").GetGuid();
        using var upload = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingestions")
        {
            Content = new StringContent(await File.ReadAllTextAsync(Path.Combine(RepoRoot(), "evals", "payroll", "employees.csv"), ct),
                Encoding.UTF8, "text/csv"),
        };
        upload.Headers.Add("X-Tenant-Id", tenant.ToString());
        (await http.SendAsync(upload, ct)).EnsureSuccessStatusCode();

        Options = new CompLedgerOptions { Endpoint = new Uri(baseUrl, "/mcp"), TenantId = tenant };
    }

    public async ValueTask DisposeAsync()
    {
        if (_api is not null) await _api.DisposeAsync();
        if (_db is not null) await _db.DisposeAsync();
        if (_image is not null) await _image.DisposeAsync();
        if (_network is not null) await _network.DisposeAsync();
    }
}

public sealed class CompLedgerTests(CompLedgerFixture fixture) : IClassFixture<CompLedgerFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly DateOnly Today = new(2026, 11, 23);

    private CompLedgerClient Client()
    {
        Assert.SkipWhen(CompLedgerFixture.Directory is null, "No comp-ledger clone at ../comp-ledger or in COMP_LEDGER_DIR.");
        return new CompLedgerClient(fixture.Options);
    }

    private static async Task<JsonElement> Invoke(LabourTools tools, string name, Dictionary<string, object?> arguments)
    {
        var function = tools.Functions().OfType<AIFunction>().Single(f => f.Name == name);
        var result = await function.InvokeAsync(new AIFunctionArguments(arguments), Ct);
        return Assert.IsType<JsonElement>(result);
    }

    [Fact]
    public async Task The_pay_in_effect_on_a_date_comes_over_MCP()
    {
        await using var client = Client();
        var march = await client.GetAsync("E-1001", new DateOnly(2026, 3, 31), Ct);
        var august = await client.GetAsync("E-1001", new DateOnly(2026, 8, 1), Ct);
        Assert.Equal(168_960m, march.Find(EmployeeCompensation.BaseSalary)!.Amount);
        Assert.Equal(180_000m, august.Find(EmployeeCompensation.BaseSalary)!.Amount);
        Assert.Equal("PLN", august.Find(EmployeeCompensation.BaseSalary)!.Currency);
    }

    [Fact]
    public async Task An_unknown_employee_is_an_error_not_an_empty_answer()
    {
        await using var client = Client();
        await Assert.ThrowsAsync<CompensationUnavailableException>(() => client.GetAsync("E-9999", Today, Ct));
    }

    [Fact]
    public async Task Overtime_pay_reads_the_base_salary_of_that_month()
    {
        await using var client = Client();
        var tools = new LabourTools(fixture.Library, client, Today);
        var result = await Invoke(tools, "overtime_pay", new()
        {
            ["year"] = 2026, ["month"] = 3, ["hours_at_50"] = 10m, ["hours_at_100"] = 4m, ["employee_id"] = "E-1001",
        });
        Assert.Equal(1_840.00m, result.GetProperty("total").GetDecimal());
        Assert.Equal(168_960m, result.GetProperty("payroll").GetProperty("annual_base_salary").GetDecimal());
        var call = Assert.Single(tools.Calls);
        Assert.False(call.Failed);
        Assert.Contains("151^1", call.BasisArticles);
        Assert.Contains(168_960m, call.Numbers);
        Assert.StartsWith("Art. 151¹ § 1. Za pracę w godzinach nadliczbowych",
            result.GetProperty("basis")[0].GetProperty("text").GetString());
    }

    [Fact]
    public async Task Notice_period_counts_from_the_first_pay_record()
    {
        await using var client = Client();
        var tools = new LabourTools(fixture.Library, client, Today);
        var result = await Invoke(tools, "notice_period", new() { ["notice_given_on"] = "2026-12-10", ["employee_id"] = "E-1003" });
        Assert.Equal("3 months", result.GetProperty("period").GetString());
        Assert.Equal("2027-03-31", result.GetProperty("ends_on").GetString());
        Assert.Equal("2019-09-01", result.GetProperty("employed_since").GetString());
    }

    [Fact]
    public async Task A_failed_lookup_comes_back_to_the_model_as_an_error()
    {
        await using var client = Client();
        var tools = new LabourTools(fixture.Library, client, Today);
        var result = await Invoke(tools, "non_compete_compensation", new() { ["ban_months"] = 6, ["employee_id"] = "E-9999" });
        Assert.True(result.TryGetProperty("error", out _));
        Assert.True(Assert.Single(tools.Calls).Failed);
    }
}
