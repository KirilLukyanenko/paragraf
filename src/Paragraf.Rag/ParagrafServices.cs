using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace Paragraf.Rag;

public sealed record ParagrafSettings
{
    public RetrievalSettings Retrieval { get; init; } = new();
    public AnswerSettings Answer { get; init; } = new();
    public string RewriteModel { get; init; } = "claude-haiku-5-5";
    public string EmbeddingModel { get; init; } = "text-embedding-3-small";
}

/// <summary>Retrieval and answering in one call: what the API and the eval runner both use.</summary>
public sealed class ParagrafEngine(Retriever retriever, Answerer answerer, ParagrafSettings defaults)
{
    public ParagrafSettings Defaults => defaults;

    public async Task<Answer> AskAsync(string question, DateOnly asOf, ParagrafSettings? settings = null, CancellationToken ct = default)
    {
        settings ??= defaults;
        var retrieval = await retriever.RetrieveAsync(question, asOf, settings.Retrieval, ct);
        return await answerer.AnswerAsync(question, asOf, retrieval, settings.Answer, ct);
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
            return anthropic.AsIChatClient(settings.Answer.Model)
                .AsBuilder()
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
        services.AddSingleton<ParagrafEngine>();
        services.AddSingleton<Ingestion>();
        return services;
    }
}
