using System.Diagnostics;
using System.Text.Json;
using System.Threading.RateLimiting;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paragraf.Api;
using Paragraf.Core.Calculations;
using Paragraf.Core.Law;
using Paragraf.Rag;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddParagraf(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(sp => new SpendGuard(
    builder.Configuration.GetValue("Paragraf:DailyBudgetUsd", 2.0m), sp.GetRequiredService<TimeProvider>()));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
builder.Services.AddProblemDetails();
// A malformed body is the client's error (400), in every environment, not an exception (500).
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteHandlerOptions>(o => o.ThrowOnBadRequest = false);

// Every question costs money: a few per minute per address is plenty for a person reading answers.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("ask", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("Paragraf:AsksPerMinute", 5),
            Window = TimeSpan.FromMinutes(1),
        }));
    o.AddPolicy("feedback", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddSingleton(sp => Build.Of(sp.GetRequiredService<LawLibrary>(), sp.GetRequiredService<ParagrafEngine>().HasPayroll));

// Traces: requests, the model and embedding calls (GenAI semantic conventions from Microsoft.Extensions.AI),
// and SQL. Exported over OTLP when OTEL_EXPORTER_OTLP_ENDPOINT is set, for example to the Aspire dashboard.
var otlp = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("paragraf"))
    .WithTracing(t =>
    {
        t.AddSource(ParagrafServices.ActivitySource)
            .AddAspNetCoreInstrumentation(o => o.Filter = ctx => ctx.Request.Path.StartsWithSegments("/api"))
            .AddHttpClientInstrumentation()
            .AddNpgsql();
        if (!string.IsNullOrEmpty(otlp)) t.AddOtlpExporter();
    });

var app = builder.Build();
var reuseAnswersFor = TimeSpan.FromDays(app.Configuration.GetValue("Paragraf:ReuseAnswersDays", 7));

// On a fresh server the store is empty: with Paragraf:IngestOnStartup the API embeds the code once (about $0.004)
// before it takes requests. Later starts find the chunks and skip it. A failure leaves /health at 503.
if (app.Configuration.GetValue<bool>("Paragraf:IngestOnStartup"))
{
    var library = app.Services.GetRequiredService<LawLibrary>();
    try
    {
        var chunks = await app.Services.GetRequiredService<ChunkStore>().CountAsync(library.Act.Eli);
        if (chunks == 0) throw new InvalidOperationException("empty");
    }
    catch (Exception ex) when (ex is InvalidOperationException or PostgresException)
    {
        try
        {
            var result = await app.Services.GetRequiredService<Ingestion>().RunAsync();
            app.Logger.LogInformation("Ingested {Chunks} chunks, ${Cost}", result.Chunks, result.CostUsd);
        }
        catch (Exception failure)
        {
            app.Logger.LogError(failure, "Ingestion on startup failed");
        }
    }
}

await app.Services.GetRequiredService<AskLog>().EnsureSchemaAsync();

// Behind the server's reverse proxy every request comes from the proxy's address, so the per-address rate limit
// would be one limit for everyone. The container is reachable only through the proxy, which sets X-Forwarded-For.
if (app.Configuration.GetValue<bool>("Paragraf:BehindProxy"))
{
    var forwarded = new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
        ForwardLimit = 1,
    };
    forwarded.KnownIPNetworks.Clear();
    forwarded.KnownProxies.Clear();
    app.UseForwardedHeaders(forwarded);
}

app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();

app.MapGet("/health", async (ChunkStore store, LawLibrary library, CancellationToken ct) =>
{
    var chunks = await store.CountAsync(library.Act.Eli, ct);
    return chunks > 0
        ? Results.Ok(new { status = "ok", act = library.Act.Eli, chunks })
        : Results.Json(new { status = "not ingested", act = library.Act.Eli, chunks }, statusCode: 503);
});

app.MapPost("/api/ask", async (AskRequest request, ParagrafEngine engine, SpendGuard spend, AskLog asks, Build build, TimeProvider clock,
    ILogger<AskRequest> logger, CancellationToken ct) =>
{
    var question = request.Question?.Trim() ?? "";
    if (question.Length is < 3 or > 1000)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["question"] = ["Ask a question of 3 to 1000 characters."] });
    }
    var asOf = request.AsOf ?? DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
    if (asOf < new DateOnly(2026, 8, 27))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["as_of"] = ["The text in use is Dz.U. 2026 poz. 1245, the law as of 27 August 2026; earlier dates need earlier texts."],
        });
    }
    var model = engine.Defaults.Answer.Model;
    var traceId = Activity.Current?.TraceId.ToString();

    // A repeated question (the page's examples, above all) gets the earlier answer: no model call, no cost, and it
    // works after the day's budget is spent. Only answers that passed every check are reused (Paragraf:ReuseAnswersDays,
    // 7 by default; 0 turns it off).
    if (reuseAnswersFor > TimeSpan.Zero
        && await asks.FindRepeatAsync(question, asOf, model, build.Version, reuseAnswersFor, ct) is { } earlier)
    {
        var repeat = JsonSerializer.Deserialize<AskResponse>(earlier.Response, ApiJson.Options)!;
        repeat = repeat with { Id = Guid.NewGuid(), CachedFrom = earlier.Id, TraceId = traceId, Usage = repeat.Usage with { CostUsd = 0 } };
        await Log(repeat, cacheable: false);
        return Results.Ok(repeat);
    }

    if (!spend.CanSpend())
    {
        return Results.Problem("The demo's daily budget for model calls is used up. It resets at midnight UTC.", statusCode: 503);
    }

    Answer answer;
    try
    {
        answer = await engine.AskAsync(question, asOf, ct: ct);
    }
    catch (Exception ex) when (ModelUnavailable(ex, ct))
    {
        // Overloaded, rate-limited, timed out or unreachable: the visitor can try again; it is not our bug (500).
        logger.LogError(ex, "The model or the embeddings did not answer");
        return Results.Problem("The model did not answer. Try again in a minute.", statusCode: 503);
    }
    var cost = answer.CostUsd + Pricing.Cost(engine.Defaults.RewriteModel, answer.Retrieval.RewriteUsage);
    spend.Record(cost);
    var response = AskResponse.From(answer, cost, traceId) with { Id = Guid.NewGuid() };
    await Log(response, cacheable: answer.Grounded && (answer.Language?.Matches ?? true));
    return Results.Ok(response);

    // The log is for the owner and the cache; an answer already paid for is returned even if the log is down.
    async Task Log(AskResponse r, bool cacheable)
    {
        try
        {
            await asks.RecordAsync(new AskLog.Entry(r.Id, clock.GetUtcNow(), question, asOf, model, build.Version, r.Status, r.Grounded,
                cacheable, r.Usage.CostUsd, r.TraceId, r.CachedFrom, Helpful: null, JsonSerializer.Serialize(r, ApiJson.Options)), CancellationToken.None);
        }
        catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Could not log the ask");
        }
    }
}).RequireRateLimiting("ask");

// A visitor's mark on an answer: helpful or not. The unhelpful ones are what `paragraf asks` lists for the eval set.
app.MapPost("/api/asks/{id:guid}/feedback", async (Guid id, FeedbackRequest request, AskLog asks, CancellationToken ct) =>
    request.Helpful is not { } helpful
        ? Results.ValidationProblem(new Dictionary<string, string[]> { ["helpful"] = ["Say true or false."] })
        : await asks.RateAsync(id, helpful, ct) ? Results.NoContent() : Results.NotFound())
    .RequireRateLimiting("feedback");

// What the page needs to know: whether calculations can read pay from the payroll system.
app.MapGet("/api/info", (ParagrafEngine engine) => Results.Ok(new { payroll = engine.HasPayroll, model = engine.Defaults.Answer.Model }));

app.MapGet("/api/articles/{number}", (string number, DateOnly? asOf, LawLibrary library, TimeProvider clock) =>
{
    if (!UnitNumber.TryParse(number, out var n)) return Results.NotFound();
    var date = asOf ?? DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
    var article = library.Act.Find(n, date);
    return article is null
        ? Results.NotFound()
        : Results.Ok(new
        {
            article = article.Key,
            display = $"art. {article.Number.ToDisplay()}",
            as_of = date,
            location = string.Join(" › ", article.Headings),
            text = article.Render(date),
            versions = library.Act.Articles.Where(a => a.Number == n).Select(a => a.Validity.ToString()),
        });
});

app.Run();

// Failures of the model or embedding services, not of this code. A cancelled request (the visitor left) is neither.
static bool ModelUnavailable(Exception ex, CancellationToken requestAborted) => ex switch
{
    OperationCanceledException => !requestAborted.IsCancellationRequested,
    Anthropic.Exceptions.AnthropicException or HttpRequestException or TimeoutException or System.ClientModel.ClientResultException => true,
    _ => false,
};

public sealed record AskRequest(string? Question, DateOnly? AsOf);

public sealed record FeedbackRequest(bool? Helpful);

/// <summary>JSON as the API writes it: snake_case. The ask log keeps responses in the same form.</summary>
public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
}

/// <summary>
/// What a cached answer depends on besides the question, the date and the model: this build of the code (the
/// assemblies' module ids change with any code change; builds are deterministic), the law text and the payroll link.
/// </summary>
public sealed record Build(string Version)
{
    public static Build Of(LawLibrary library, bool payroll) => new(string.Join("-",
        new[] { typeof(Answerer), typeof(LabourCalculations), typeof(Build) }
            .Select(t => t.Assembly.ManifestModule.ModuleVersionId.ToString("N")[..8])
            .Append(library.Act.Eli)
            .Append(payroll ? "payroll" : "no-payroll")));
}

public sealed record AskResponse(
    Guid Id,
    Guid? CachedFrom,
    string Status,
    string Answer,
    DateOnly AsOf,
    bool Grounded,
    IReadOnlyList<AskResponse.CitationDto> Citations,
    IReadOnlyList<AskResponse.SourceDto> Sources,
    string? SearchQuery,
    IReadOnlyList<AskResponse.CalculationDto> Calculations,
    IReadOnlyList<AskResponse.AmountDto> Amounts,
    AskResponse.LanguageDto? Language,
    AskResponse.UsageDto Usage,
    string? TraceId)
{
    public sealed record CalculationDto(string Tool, JsonElement Arguments, JsonElement Result, bool Failed, double Ms);
    public sealed record AmountDto(string Text, decimal Value, bool Verified);
    public sealed record LanguageDto(string? Question, string? Answer, bool Rewritten);
    public sealed record CitationDto(string Article, string? Paragraph, string? Display, string Quote, bool Verified, string? Problem);
    public sealed record SourceDto(string Article, string Display, string Location);
    public sealed record UsageDto(string Model, long? InputTokens, long? OutputTokens, decimal CostUsd, double RetrievalMs, double AnswerMs);

    public static AskResponse From(Answer a, decimal cost, string? traceId) => new(
        Guid.Empty,
        null,
        a.Status switch
        {
            AnswerStatus.Answered => "answered",
            AnswerStatus.NotInSources => "not_in_sources",
            AnswerStatus.OutOfScope => "out_of_scope",
            _ => "refused",
        },
        a.Text,
        a.AsOf,
        a.Grounded,
        a.Citations.Select(c => new CitationDto(c.Citation.Article, c.Citation.Paragraph, c.Display, c.Citation.Quote, c.Verified,
            c.Verified ? null : c.Problem.ToString())).ToList(),
        a.Retrieval.Articles.Select(s => new SourceDto(s.Article.Key, $"art. {s.Article.Number.ToDisplay()}", string.Join(" › ", s.Article.Headings))).ToList(),
        a.Retrieval.Rewrite?.PolishQuery,
        a.ToolCalls.Select(c => new CalculationDto(c.Name, c.Arguments, c.Result, c.Failed, Math.Round(c.Elapsed.TotalMilliseconds))).ToList(),
        a.Amounts.Select(x => new AmountDto(x.Text, x.Value, x.Verified)).ToList(),
        a.Language is { } l ? new LanguageDto(l.Question, l.Answer, l.Rewritten) : null,
        new UsageDto(a.Model, a.Usage?.InputTokenCount, a.Usage?.OutputTokenCount, Math.Round(cost, 5),
            Math.Round(a.Retrieval.Elapsed.TotalMilliseconds), Math.Round(a.GenerationTime.TotalMilliseconds)),
        traceId);
}

/// <summary>For WebApplicationFactory in the API tests.</summary>
public partial class Program;
