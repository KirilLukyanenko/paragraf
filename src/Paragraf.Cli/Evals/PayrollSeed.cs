using System.Net.Http.Json;
using System.Text;

namespace Paragraf.Cli.Evals;

/// <summary>Loads the demo employees into comp-ledger through its REST API (MCP has no upload tool on purpose).</summary>
public static class PayrollSeed
{
    public static async Task<Guid> SeedAsync(Uri baseUrl, string csv, CancellationToken ct)
    {
        using var http = new HttpClient { BaseAddress = baseUrl };
        var created = await http.PostAsJsonAsync("/api/v1/tenants", new { name = "paragraf demo" }, ct);
        created.EnsureSuccessStatusCode();
        var tenant = (await created.Content.ReadFromJsonAsync<Tenant>(ct))!.Id;

        using var upload = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingestions")
        {
            Content = new StringContent(csv, Encoding.UTF8, "text/csv"),
        };
        upload.Headers.Add("X-Tenant-Id", tenant.ToString());
        var applied = await http.SendAsync(upload, ct);
        if (!applied.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"comp-ledger rejected the file ({(int)applied.StatusCode}): {await applied.Content.ReadAsStringAsync(ct)}");
        }
        return tenant;
    }

    private sealed record Tenant(Guid Id, string Name);
}
