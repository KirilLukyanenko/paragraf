using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;
using Paragraf.Rag.Payroll;

namespace Paragraf.Rag;

public sealed record ParagrafSettings
{
    public RetrievalSettings Retrieval { get; init; } = new();
    public AnswerSettings Answer { get; init; } = new();
    public string RewriteModel { get; init; } = "claude-haiku-5-5";
    public string EmbeddingModel { get; init; } = "text-embedding-3-small";
}

/// <summary>Retrieval and answering in one call: what the API and the eval runner both use.</summary>
public sealed class ParagrafEngine(
    Retriever retriever, Answerer answerer, LawLibrary library, ParagrafSettings defaults, ICompensationSource? compensation = null)
{
    public ParagrafSettings Defaults => defaults;

    /// <summary>Whether a payroll system (comp-ledger) is connected.</summary>
    public bool HasPayroll => compensation is not null;

    public async Task<Answer> AskAsync(string question, DateOnly asOf, ParagrafSettings? settings = null, CancellationToken ct = default)
    {
        settings ??= defaults;
        var retrieval = await retriever.RetrieveAsync(question, asOf, settings.Retrieval, ct);
        var tools = settings.Answer.Tools ? new LabourTools(library, compensation, asOf) : null;
        return await answerer.AnswerAsync(question, asOf, retrieval, settings.Answer, tools, ct);
    }
}

public static class ParagrafServices
{
    public const string ActivitySource = "Paragraf";

    /// <summary>
    /// Registers the engine. Models go through Microsoft.Extensions.AI: Claude via the Anthropic SDK's
    /// <c>AsIChatClient</c>, embeddings via OpenAI's <c>AsIEmbeddingGenerator</c>, both wrapped in the
    /// OpenTelemetry middleware (GenAI semantic conventions: model, tokens, latency per call).
    /// </summary>
    public static IServiceCollection AddParagraf(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Paragraf").Get<ParagrafSettings>() ?? new ParagrafSettings();
        services.AddSingleton(settings);

        services.AddSingleton(_ => LawLibrary.KodeksPracy(configuration["Paragraf:DataDirectory"]
            ?? LawLibrary.FindDataDirectory(AppContext.BaseDirectory, Environment.CurrentDirectory)));

        services.AddSingleton(_ => ChunkStore.CreateDataSource(configuration.GetConnectionString("Paragraf")
            ?? throw new InvalidOperationException("ConnectionStrings:Paragraf is not set.")));
        services.AddSingleton<ChunkStore>();

        services.AddSingleton<IChatClient>(sp =>
        {
            var apiKey = configuration["Anthropic:ApiKey"] ?? configuration["ANTHROPIC_API_KEY"]
                ?? throw new InvalidOperationException("Anthropic:ApiKey (or ANTHROPIC_API_KEY) is not set.");
            var anthropic = new AnthropicClient { ApiKey = apiKey };
            // Function invocation outside telemetry: each model call of a tool loop is its own span.
            return anthropic.AsIChatClient(settings.Answer.Model)
                .AsBuilder()
                .UseFunctionInvocation(sp.GetService<ILoggerFactory>(), f => f.MaximumIterationsPerRequest = 8)
                .UseOpenTelemetry(sp.GetService<ILoggerFactory>(), ActivitySource)
                .Build(sp);
        });
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var apiKey = configuration["OpenAI:ApiKey"] ?? configuration["OPENAI_API_KEY"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey (or OPENAI_API_KEY) is not set.");
            return new OpenAIClient(apiKey).GetEmbeddingClient(settings.EmbeddingModel).AsIEmbeddingGenerator()
                .AsBuilder()
                .UseOpenTelemetry(sp.GetService<ILoggerFactory>(), ActivitySource)
                .Build(sp);
        });

        services.AddSingleton(sp => new QueryRewriter(sp.GetRequiredService<IChatClient>(), settings.RewriteModel));
        services.AddSingleton<Retriever>();
        services.AddSingleton<Answerer>();
        // comp-ledger over MCP, when configured: CompLedger:Endpoint (its /mcp URL) and CompLedger:TenantId.
        var compLedger = configuration.GetSection("CompLedger").Get<CompLedgerOptions>();
        if (compLedger?.Endpoint is not null)
        {
            services.AddSingleton(compLedger);
            services.AddSingleton<CompLedgerClient>();
            services.AddSingleton<ICompensationSource>(sp => sp.GetRequiredService<CompLedgerClient>());
        }
        services.AddSingleton(sp => new ParagrafEngine(
            sp.GetRequiredService<Retriever>(), sp.GetRequiredService<Answerer>(), sp.GetRequiredService<LawLibrary>(),
            settings, sp.GetService<ICompensationSource>()));
        services.AddSingleton<Ingestion>();
        return services;
    }
}
