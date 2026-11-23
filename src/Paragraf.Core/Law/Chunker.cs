namespace Paragraf.Core.Law;

/// <summary>
/// A unit of search: one paragraph of an article, or a whole article that has no paragraphs.
/// The answer model never sees chunks alone: retrieval finds chunks, then the whole articles they
/// belong to go into the prompt ("small to big"), so a paragraph that says "w przypadkach
/// określonych w § 1" arrives with its § 1.
/// </summary>
public sealed record Chunk(
    string Id,
    string Article,
    string? Paragraph,
    Validity Validity,
    string Headings,
    string Body,
    string Content)
{
    /// <summary>"art. 151¹ § 1" — how people cite it.</summary>
    public string Citation => Paragraph is null
        ? $"art. {UnitNumber.Parse(Article).ToDisplay()}"
        : $"art. {UnitNumber.Parse(Article).ToDisplay()} § {UnitNumber.Parse(Paragraph).ToDisplay()}";
}

public static class Chunker
{
    public static IReadOnlyList<Chunk> Chunk(Act act)
    {
        var chunks = new List<Chunk>();
        foreach (var article in act.Articles.Where(a => !a.Repealed))
        {
            var headings = string.Join(" › ", article.Headings);
            var head = $"Art. {article.Number.ToDisplay()}.";
            if (article.Paragraphs.Count == 0)
            {
                chunks.Add(Make(act, article, null, article.Validity, headings, $"{head} {article.Lead}"));
                continue;
            }
            foreach (var paragraph in article.Paragraphs.Where(p => !p.Repealed))
            {
                var lead = string.IsNullOrEmpty(article.Lead) ? "" : article.Lead + " ";
                var body = $"{head} {lead}§ {paragraph.Number.ToDisplay()}. {paragraph.Text}";
                chunks.Add(Make(act, article, paragraph.Number.ToString(), Intersect(article.Validity, paragraph.Validity), headings, body));
            }
        }
        return chunks;
    }

    private static Chunk Make(Act act, Article article, string? paragraph, Validity validity, string headings, string body)
    {
        var id = paragraph is null ? article.Key : $"{article.Key}§{paragraph}";
        if (validity.From is { } from) id += $"@{from:yyyy-MM-dd}";
        else if (validity.Until is { } until) id += $"@until-{until:yyyy-MM-dd}";
        // The headings travel with the text into the embedding: "Czas pracy › Praca w godzinach nadliczbowych"
        // says what a paragraph is about when its own words don't.
        var content = $"{act.Title} › {headings}\n{body}";
        return new Chunk(id, article.Key, paragraph, validity, headings, body, content);
    }

    private static Validity Intersect(Validity a, Validity b) => new(
        Max(a.From, b.From),
        Min(a.Until, b.Until));

    private static DateOnly? Max(DateOnly? a, DateOnly? b) => a is null ? b : b is null ? a : a > b ? a : b;
    private static DateOnly? Min(DateOnly? a, DateOnly? b) => a is null ? b : b is null ? a : a < b ? a : b;
}
