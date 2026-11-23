using Microsoft.Extensions.AI;

namespace Paragraf.Rag;

/// <summary>Embeds every chunk of the act and replaces it in the store.</summary>
public sealed class Ingestion(LawLibrary library, ChunkStore store, IEmbeddingGenerator<string, Embedding<float>> embeddings, ParagrafSettings settings)
{
    public sealed record Result(int Chunks, long Tokens, decimal CostUsd);

    public async Task<Result> RunAsync(Action<string>? progress = null, CancellationToken ct = default)
    {
        await store.EnsureSchemaAsync(ct);
        var chunks = library.Chunks;
        var rows = new List<(Paragraf.Core.Law.Chunk, float[])>(chunks.Count);
        long tokens = 0;
        foreach (var batch in chunks.Chunk(128))
        {
            var generated = await embeddings.GenerateAsync(batch.Select(c => c.Content), cancellationToken: ct);
            tokens += generated.Usage?.InputTokenCount ?? 0;
            rows.AddRange(batch.Zip(generated, (c, e) => (c, e.Vector.ToArray())));
            progress?.Invoke($"embedded {rows.Count}/{chunks.Count}");
        }
        await store.ReplaceAsync(library.Act.Eli, settings.EmbeddingModel, rows, ct);
        var cost = Pricing.Cost(settings.EmbeddingModel, new UsageDetails { InputTokenCount = tokens });
        return new Result(rows.Count, tokens, cost);
    }
}
