using System.Diagnostics;
using System.Text.Json;
using System.Threading.RateLimiting;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Paragraf.Api;
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
});

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

app.MapPost("/api/ask", async (AskRequest request, ParagrafEngine engine, SpendGuard spend, TimeProvider clock, CancellationToken ct) =>
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
    if (!spend.CanSpend())
    {
        return Results.Problem("The demo's daily budget for model calls is used up. It resets at midnight UTC.", statusCode: 503);
    }

    var answer = await engine.AskAsync(question, asOf, ct: ct);
    var cost = answer.CostUsd + Pricing.Cost(engine.Defaults.RewriteModel, answer.Retrieval.RewriteUsage);
    spend.Record(cost);
    return Results.Ok(AskResponse.From(answer, cost, Activity.Current?.TraceId.ToString()));
}).RequireRateLimiting("ask");

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

public sealed record AskRequest(string? Question, DateOnly? AsOf);

public sealed record AskResponse(
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
