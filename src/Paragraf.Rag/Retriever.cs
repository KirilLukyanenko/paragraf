using System.Diagnostics;
using Microsoft.Extensions.AI;
using Paragraf.Core.Law;
using Paragraf.Core.Search;

namespace Paragraf.Rag;

/// <summary>Which parts of the hybrid search run. The eval set compares them (ablations).</summary>
public sealed record RetrievalSettings
{
    public bool Vector { get; init; } = true;
    public bool Lexical { get; init; } = true;
    public bool NamedArticles { get; init; } = true;
    public bool Rewrite { get; init; } = true;
    public int CandidatesPerList { get; init; } = 30;
    /// <summary>Weight of the BM25 list in rank fusion (the vector list weighs 1).</summary>
    public double LexicalWeight { get; init; } = 0.5;
    /// <summary>How many distinct articles go into the prompt.</summary>
    public int Articles { get; init; } = 8;

    public string Describe() => string.Join('+', new[]
    {
        Vector ? "vector" : null, Lexical ? $"bm25x{LexicalWeight}" : null, NamedArticles ? "named" : null, Rewrite ? "rewrite" : null,
    }.Where(s => s is not null)) + $"/k{Articles}";
}

public sealed record RetrievedArticle(Article Article, IReadOnlyList<string> MatchedChunks, double Score);

public sealed record Retrieval(
    IReadOnlyList<RetrievedArticle> Articles,
    IReadOnlyList<RankFusion.Ranked> RankedChunks,
    RewrittenQuery? Rewrite,
    TimeSpan Elapsed,
    UsageDetails? RewriteUsage);

/// <summary>
/// Hybrid retrieval: nearest chunks by embedding, chunks by lexical terms, and articles the question
/// names, fused by reciprocal rank; then the chunks are grouped into whole articles as they read on
/// the date ("small to big").
/// </summary>
public sealed class Retriever(
    LawLibrary library,
    ChunkStore store,
    IEmbeddingGenerator<string, Embedding<float>> embeddings,
    QueryRewriter rewriter)
{
    public async Task<Retrieval> RetrieveAsync(string question, DateOnly date, RetrievalSettings settings, CancellationToken ct = default)
    {
        var watch = Stopwatch.StartNew();
        var act = library.Act.Eli;
        RewrittenQuery? rewrite = null;
        UsageDetails? rewriteUsage = null;
        if (settings.Rewrite)
        {
            (rewrite, rewriteUsage) = await rewriter.RewriteAsync(question, ct);
        }
        var searchText = rewrite is null ? question : $"{rewrite.PolishQuery}\n{string.Join(' ', rewrite.Keywords)}";

        var lists = new List<(string Name, IReadOnlyList<string> Ids, double Weight)>();
        var vectorTask = settings.Vector
            ? EmbedAndSearch(act, rewrite is null ? question : rewrite.PolishQuery, date, settings.CandidatesPerList, ct)
            : Task.FromResult<IReadOnlyList<ChunkStore.Hit>>([]);
        var lexical = settings.Lexical
            ? library.Bm25.Search(searchText, settings.CandidatesPerList, id => library.Chunk(id)!.Validity.IsInForceOn(date)).Select(h => h.Id).ToList()
            : [];
        var vector = await vectorTask;

        if (settings.Vector) lists.Add(("vector", vector.Select(h => h.Id).ToList(), 1.0));
        if (settings.Lexical) lists.Add(("lexical", lexical, settings.LexicalWeight));
        if (settings.NamedArticles)
        {
            var named = NamedChunks(question, date).Concat(rewrite is null ? [] : NamedChunks(string.Join(' ', rewrite.Articles.Select(a => "art. " + a)), date)).Distinct().ToList();
            // A named article is what the user asked about: it outranks anything a search can find.
            if (named.Count > 0) lists.Add(("named", named, 3.0));
        }

        var fused = RankFusion.Fuse(lists);
        var articles = new List<RetrievedArticle>();
        foreach (var group in fused.GroupBy(r => library.Chunk(r.Id)?.Article))
        {
            if (group.Key is null) continue;
            var article = library.Act.Find(UnitNumber.Parse(group.Key), date);
            if (article is null || article.Repealed) continue;
            articles.Add(new RetrievedArticle(article, group.Select(r => r.Id).ToList(), group.Max(r => r.Score)));
        }
        var top = articles.OrderByDescending(a => a.Score).Take(settings.Articles).ToList();
        return new Retrieval(top, fused, rewrite, watch.Elapsed, rewriteUsage);
    }

    private async Task<IReadOnlyList<ChunkStore.Hit>> EmbedAndSearch(string act, string text, DateOnly date, int limit, CancellationToken ct)
    {
        var vector = await embeddings.GenerateVectorAsync(text, cancellationToken: ct);
        return await store.NearestAsync(act, vector.ToArray(), date, limit, ct);
    }

    /// <summary>Chunks of the articles a text names, in force on the date; a named paragraph first.</summary>
    private IEnumerable<string> NamedChunks(string text, DateOnly date)
    {
        foreach (var reference in ArticleReference.Find(text))
        {
            var article = library.Act.Find(reference.Article, date);
            if (article is null) continue;
            var chunks = library.Chunks
                .Where(c => c.Article == article.Key && c.Validity.IsInForceOn(date))
                .OrderBy(c => reference.Paragraph is { } p && c.Paragraph == p.ToString() ? 0 : 1)
                .Select(c => c.Id);
            foreach (var id in chunks) yield return id;
        }
    }
}
