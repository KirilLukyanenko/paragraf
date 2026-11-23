namespace Paragraf.Core.Search;

/// <summary>
/// Okapi BM25 over the chunks, in memory. The first lexical retriever was PostgreSQL's
/// <c>ts_rank_cd</c> over an OR of the terms; it has no inverse document frequency, so a chunk full of
/// common words ("pracownik", "pracodawca") outranked the one with the rare word that mattered, and
/// the eval set measured it at 33% recall. The act is about 1,100 chunks, so an in-process index
/// costs nothing and is rebuilt at start from the same text.
/// </summary>
public sealed class Bm25Index
{
    private readonly double _k1;
    private readonly double _b;
    private readonly IReadOnlyList<string> _ids;
    private readonly List<Dictionary<string, int>> _termFrequencies = [];
    private readonly List<int> _lengths = [];
    private readonly Dictionary<string, int> _documentFrequency = new(StringComparer.Ordinal);
    private readonly double _averageLength;

    public Bm25Index(IEnumerable<(string Id, string Text)> documents, int prefixLength = LexicalTerms.PrefixLength, double k1 = 1.2, double b = 0.75)
    {
        _k1 = k1;
        _b = b;
        PrefixLength = prefixLength;
        var ids = new List<string>();
        foreach (var (id, text) in documents)
        {
            var terms = LexicalTerms.Of(text, prefixLength);
            var tf = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var t in terms) tf[t] = tf.GetValueOrDefault(t) + 1;
            foreach (var t in tf.Keys) _documentFrequency[t] = _documentFrequency.GetValueOrDefault(t) + 1;
            ids.Add(id);
            _termFrequencies.Add(tf);
            _lengths.Add(terms.Count);
        }
        _ids = ids;
        _averageLength = _lengths.Count == 0 ? 0 : _lengths.Average();
    }

    public int PrefixLength { get; }

    public IReadOnlyList<(string Id, double Score)> Search(string query, int limit, Func<string, bool>? filter = null)
    {
        var queryTerms = LexicalTerms.Of(query, PrefixLength).Distinct().ToList();
        var n = _ids.Count;
        var scores = new List<(string Id, double Score)>();
        for (var i = 0; i < n; i++)
        {
            if (filter is not null && !filter(_ids[i])) continue;
            var tf = _termFrequencies[i];
            double score = 0;
            foreach (var term in queryTerms)
            {
                if (!tf.TryGetValue(term, out var f)) continue;
                var df = _documentFrequency[term];
                var idf = Math.Log(1 + (n - df + 0.5) / (df + 0.5));
                score += idf * f * (_k1 + 1) / (f + _k1 * (1 - _b + _b * _lengths[i] / _averageLength));
            }
            if (score > 0) scores.Add((_ids[i], score));
        }
        return scores.OrderByDescending(s => s.Score).ThenBy(s => s.Id, StringComparer.Ordinal).Take(limit).ToList();
    }
}
