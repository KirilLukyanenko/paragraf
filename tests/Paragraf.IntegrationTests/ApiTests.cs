using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Paragraf.Rag;
using Testcontainers.PostgreSql;

namespace Paragraf.IntegrationTests;

/// <summary>
/// The API in process (WebApplicationFactory) over a real pgvector with the code's chunks loaded. The models are
/// replaced: a scripted answer model and fixed embeddings, so the tests cost nothing and check the HTTP contract:
/// validation, the rate limit, the daily spend ceiling, the JSON shape and the article endpoint.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("pgvector/pgvector:pg17")
        .WithPassword(Guid.NewGuid().ToString("N")).Build();

    public static string RepoRoot([System.Runtime.CompilerServices.CallerFilePath] string file = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, "..", ".."));

    public string ConnectionString => _db.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await _db.StartAsync(ct);
        var library = LawLibrary.KodeksPracy(Path.Combine(RepoRoot(), "data"));
        var store = new ChunkStore(ChunkStore.CreateDataSource(ConnectionString));
        await store.RecreateSchemaAsync(ct: ct);
        var vector = new float[ChunkStore.Dimensions];
        vector[0] = 1f;
        await store.ReplaceAsync(library.Act.Eli, "test-embeddings", library.Chunks.Select(c => (c, vector)).ToList(), ct);
    }

    public async ValueTask DisposeAsync() => await _db.DisposeAsync();

    /// <summary>
    /// An API with its own state (rate limiter, spend guard) and the given model and settings. The ask log is in the
    /// shared database, so reusing answers is off unless a test asks for it (and then uses a question of its own).
    /// </summary>
    public WebApplicationFactory<Program> Api(ScriptedModel model, int asksPerMinute = 100, decimal dailyBudgetUsd = 10m, bool behindProxy = false,
        bool reuseAnswers = false) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
        {
            web.UseEnvironment("Testing");
            web.UseSetting("ConnectionStrings:Paragraf", ConnectionString);
            web.UseSetting("Paragraf:DataDirectory", Path.Combine(RepoRoot(), "data"));
            web.UseSetting("Paragraf:Retrieval:Rewrite", "false");
            web.UseSetting("Paragraf:AsksPerMinute", asksPerMinute.ToString());
            web.UseSetting("Paragraf:DailyBudgetUsd", dailyBudgetUsd.ToString(System.Globalization.CultureInfo.InvariantCulture));
            web.UseSetting("Paragraf:BehindProxy", behindProxy.ToString());
            web.UseSetting("Paragraf:ReuseAnswersDays", reuseAnswers ? "7" : "0");
            web.UseSetting("Anthropic:ApiKey", "not-used");
            web.UseSetting("OpenAI:ApiKey", "not-used");
            web.ConfigureTestServices(services =>
            {
                services.AddSingleton<IChatClient>(model);
                services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(new FixedEmbeddings());
            });
        });
}

public sealed class ApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    private static Task<HttpResponseMessage> Ask(HttpClient client, string question, string? asOf = "2026-11-23") =>
        client.PostAsync("/api/ask", Json(JsonSerializer.Serialize(new { question, as_of = asOf })), Ct);

    [Fact]
    public async Task Health_reports_the_loaded_chunks()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()));
        var health = await api.CreateClient().GetFromJsonAsync<JsonElement>("/health", Ct);
        Assert.Equal("ok", health.GetProperty("status").GetString());
        Assert.True(health.GetProperty("chunks").GetInt32() > 1000);
    }

    [Fact]
    public async Task An_answer_comes_back_in_snake_case_with_verified_citations_and_its_cost()
    {
        var model = new ScriptedModel(ScriptedModel.NoticeAnswer());
        await using var api = fixture.Api(model);
        var response = await Ask(api.CreateClient(), "What notice period applies under art. 36 after 7 years with the employer?");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.Equal("answered", body.GetProperty("status").GetString());
        Assert.True(body.GetProperty("grounded").GetBoolean());
        var citation = body.GetProperty("citations")[0];
        Assert.Equal("art. 36 § 1", citation.GetProperty("display").GetString());
        Assert.True(citation.GetProperty("verified").GetBoolean());
        Assert.Contains(body.GetProperty("sources").EnumerateArray(), s => s.GetProperty("article").GetString() == "36");
        Assert.Equal("en", body.GetProperty("language").GetProperty("question").GetString());
        Assert.Equal(0.006m, body.GetProperty("usage").GetProperty("cost_usd").GetDecimal()); // 1,000 in and 100 out on Opus
        Assert.Contains("<question language=\"English\">", model.Requests.Single().Last().Text);
    }

    [Theory]
    [InlineData("""{"question":"?"}""", "question")]
    [InlineData("""{"question":"Co to jest mobbing?","as_of":"2026-01-01"}""", "as_of")]
    public async Task Bad_input_is_a_validation_problem(string json, string field)
    {
        var model = new ScriptedModel(ScriptedModel.NoticeAnswer());
        await using var api = fixture.Api(model);
        var response = await api.CreateClient().PostAsync("/api/ask", Json(json), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _));
        Assert.Empty(model.Requests); // nothing was spent
    }

    [Fact]
    public async Task A_malformed_body_is_400_not_500()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()));
        var response = await api.CreateClient().PostAsync("/api/ask", Json("{\"question\": "), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Asks_over_the_limit_get_429()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()), asksPerMinute: 2);
        var client = api.CreateClient();
        var codes = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++) codes.Add((await Ask(client, "What notice period does art. 36 give after 7 years?")).StatusCode);
        Assert.Equal([HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests], codes);
    }

    [Fact]
    public async Task Behind_the_proxy_the_limit_is_per_visitor_address()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()), asksPerMinute: 1, behindProxy: true);
        var client = api.CreateClient();
        async Task<HttpStatusCode> AskFrom(string address)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ask")
            {
                Content = Json(JsonSerializer.Serialize(new { question = "What notice period does art. 36 give after 7 years?" })),
            };
            request.Headers.Add("X-Forwarded-For", address);
            return (await client.SendAsync(request, Ct)).StatusCode;
        }
        Assert.Equal(HttpStatusCode.OK, await AskFrom("203.0.113.7"));
        Assert.Equal(HttpStatusCode.OK, await AskFrom("198.51.100.20"));
        Assert.Equal(HttpStatusCode.TooManyRequests, await AskFrom("203.0.113.7"));
    }

    [Fact]
    public async Task When_the_day_s_budget_is_spent_the_API_answers_503_without_calling_the_model()
    {
        var model = new ScriptedModel(ScriptedModel.NoticeAnswer());
        await using var api = fixture.Api(model, dailyBudgetUsd: 0.005m); // one answer costs 0.006
        var client = api.CreateClient();
        const string question = "What notice period does art. 36 give after 7 years?";
        Assert.Equal(HttpStatusCode.OK, (await Ask(client, question)).StatusCode);
        var second = await Ask(client, question);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, second.StatusCode);
        Assert.Single(model.Requests);
    }

    [Fact]
    public async Task An_article_reads_as_it_was_on_the_date()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()));
        var client = api.CreateClient();
        var before = await client.GetFromJsonAsync<JsonElement>("/api/articles/94%5E3?asOf=2026-11-01", Ct);
        var after = await client.GetFromJsonAsync<JsonElement>("/api/articles/94%5E3?asOf=2026-11-23", Ct);
        Assert.Contains("uporczywym i długotrwałym nękaniu", before.GetProperty("text").GetString());
        Assert.DoesNotContain("długotrwałym", after.GetProperty("text").GetString());
        Assert.Equal(2, after.GetProperty("versions").GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/articles/999", Ct)).StatusCode);
    }

    private AskLog Log() => new(ChunkStore.CreateDataSource(fixture.ConnectionString), TimeProvider.System);

    [Fact]
    public async Task A_repeated_question_gets_the_earlier_answer_free_even_when_the_budget_is_spent()
    {
        var model = new ScriptedModel(ScriptedModel.NoticeAnswer());
        await using var api = fixture.Api(model, dailyBudgetUsd: 0.005m, reuseAnswers: true); // one answer costs 0.006
        var client = api.CreateClient();
        var question = $"What notice period does art. 36 give after 7 years? ({Guid.NewGuid():N})";

        var first = await (await Ask(client, question)).Content.ReadFromJsonAsync<JsonElement>(Ct);
        // Other spacing and case is the same question.
        var again = await Ask(client, "  " + question.ToUpperInvariant().Replace(" ", "   "));
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        var second = await again.Content.ReadFromJsonAsync<JsonElement>(Ct);

        Assert.Single(model.Requests);
        Assert.Equal(first.GetProperty("id").GetGuid(), second.GetProperty("cached_from").GetGuid());
        Assert.NotEqual(first.GetProperty("id").GetGuid(), second.GetProperty("id").GetGuid());
        Assert.Equal(0m, second.GetProperty("usage").GetProperty("cost_usd").GetDecimal());
        Assert.Equal(first.GetProperty("answer").GetString(), second.GetProperty("answer").GetString());
        Assert.Equal(JsonValueKind.Null, first.GetProperty("cached_from").ValueKind);

        // A new question still meets the spent budget.
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await Ask(client, question + " And after 2 years?")).StatusCode);
    }

    [Fact]
    public async Task An_answer_that_failed_a_check_is_not_reused()
    {
        var bad = new
        {
            status = "answered",
            answer = "The notice period is 3 months.",
            citations = new[] { new { article = "36", paragraph = "1", quote = "words that are not in the code" } },
        };
        var model = new ScriptedModel(bad);
        await using var api = fixture.Api(model, reuseAnswers: true);
        var client = api.CreateClient();
        var question = $"What notice period does art. 36 give? ({Guid.NewGuid():N})";
        var first = await (await Ask(client, question)).Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.False(first.GetProperty("grounded").GetBoolean());
        await Ask(client, question);
        Assert.Equal(2, model.Requests.Count);
    }

    [Fact]
    public async Task Every_ask_is_logged_and_a_visitor_can_mark_it_unhelpful()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()));
        var client = api.CreateClient();
        var question = $"What notice period does art. 36 give after 7 years? ({Guid.NewGuid():N})";
        var body = await (await Ask(client, question)).Content.ReadFromJsonAsync<JsonElement>(Ct);
        var id = body.GetProperty("id").GetGuid();

        var feedback = await client.PostAsync($"/api/asks/{id}/feedback", Json("""{"helpful":false}"""), Ct);
        Assert.Equal(HttpStatusCode.NoContent, feedback.StatusCode);

        var entry = (await Log().SinceAsync(DateTimeOffset.UtcNow.AddMinutes(-5), Ct)).Single(e => e.Id == id);
        Assert.Equal(question, entry.Question);
        Assert.Equal("answered", entry.Status);
        Assert.True(entry.Grounded && entry.Cacheable);
        Assert.Equal(0.006m, entry.CostUsd);
        Assert.False(entry.Helpful);
        Assert.Equal(id, JsonDocument.Parse(entry.Response).RootElement.GetProperty("id").GetGuid());

        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/api/asks/{Guid.NewGuid()}/feedback", Json("""{"helpful":true}"""), Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/api/asks/{id}/feedback", Json("{}"), Ct)).StatusCode);
    }

    [Fact]
    public async Task When_the_model_is_down_the_API_answers_503_and_spends_nothing()
    {
        var model = new ScriptedModel(new HttpRequestException("529 overloaded"));
        await using var api = fixture.Api(model);
        var response = await Ask(api.CreateClient(), "What notice period does art. 36 give after 7 years?");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.Contains("did not answer", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Info_says_whether_payroll_is_connected()
    {
        await using var api = fixture.Api(new ScriptedModel(ScriptedModel.NoticeAnswer()));
        var info = await api.CreateClient().GetFromJsonAsync<JsonElement>("/api/info", Ct);
        Assert.False(info.GetProperty("payroll").GetBoolean());
    }
}
