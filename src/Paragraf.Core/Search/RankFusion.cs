namespace Paragraf.Core.Search;

/// <summary>
/// Reciprocal rank fusion: merges ranked lists from different retrievers (vector, full-text, named
/// articles) without calibrating their scores against each other. Each list adds
/// <c>weight / (k + rank)</c> for every id it ranks; k = 60 is the value from the original paper.
/// </summary>
public static class RankFusion
{
    public sealed record Ranked(string Id, double Score, IReadOnlyDictionary<string, int> Ranks);

    public static IReadOnlyList<Ranked> Fuse(IEnumerable<(string Name, IReadOnlyList<string> Ids, double Weight)> lists, int k = 60)
    {
        var scores = new Dictionary<string, double>();
        var ranks = new Dictionary<string, Dictionary<string, int>>();
        foreach (var (name, ids, weight) in lists)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                if (!ranks.TryGetValue(id, out var byList)) ranks[id] = byList = [];
                if (byList.ContainsKey(name)) continue; // a list naming an id twice counts once, at its best rank
                byList[name] = i + 1;
                scores[id] = scores.GetValueOrDefault(id) + weight / (k + i + 1);
            }
        }
        return scores
            .OrderByDescending(s => s.Value)
            .ThenBy(s => s.Key, StringComparer.Ordinal)
            .Select(s => new Ranked(s.Key, s.Value, ranks[s.Key]))
            .ToList();
    }
}
