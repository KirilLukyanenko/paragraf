namespace Paragraf.Core.Law;

/// <summary>
/// When a version of an article or paragraph is in force: from <see cref="From"/> (inclusive) until
/// <see cref="Until"/> (exclusive). Null means open. A consolidated text can carry two versions of
/// the same unit: the one in force now and the one an amendment brings in on a later date.
/// </summary>
public sealed record Validity(DateOnly? From, DateOnly? Until)
{
    public static readonly Validity Always = new(null, null);

    public bool IsInForceOn(DateOnly date) => (From is null || date >= From) && (Until is null || date < Until);

    public override string ToString() => (From, Until) switch
    {
        (null, null) => "always",
        (null, { } until) => $"until {until:yyyy-MM-dd}",
        ({ } from, null) => $"from {from:yyyy-MM-dd}",
        ({ } from, { } until) => $"{from:yyyy-MM-dd} – {until:yyyy-MM-dd}",
    };
}

/// <summary>A paragraph (§) of an article. An article without paragraphs has none of these.</summary>
public sealed record Paragraph(UnitNumber Number, string Text, bool Repealed, Validity Validity);

/// <summary>
/// One article as the consolidated text states it. <see cref="Lead"/> is its text when it has no
/// paragraphs, or the words before the first paragraph when it has some (usually empty).
/// </summary>
public sealed record Article(
    UnitNumber Number,
    string Lead,
    IReadOnlyList<Paragraph> Paragraphs,
    IReadOnlyList<string> Headings,
    bool Repealed,
    Validity Validity)
{
    public string Key => Number.ToString();

    /// <summary>Paragraphs in force on a date: at most one version of each number.</summary>
    public IEnumerable<Paragraph> ParagraphsOn(DateOnly date) => Paragraphs.Where(p => p.Validity.IsInForceOn(date));

    /// <summary>The article as it reads on a date, as plain text with its paragraphs.</summary>
    public string Render(DateOnly date)
    {
        var lines = new List<string>();
        var head = $"Art. {Number.ToDisplay()}.";
        if (Repealed) return $"{head} (uchylony)";
        var paragraphs = ParagraphsOn(date).ToList();
        if (paragraphs.Count == 0) return $"{head} {Lead}".TrimEnd();
        lines.Add(string.IsNullOrEmpty(Lead) ? head : $"{head} {Lead}");
        foreach (var p in paragraphs)
        {
            lines.Add(p.Repealed ? $"§ {p.Number.ToDisplay()}. (uchylony)" : $"§ {p.Number.ToDisplay()}. {p.Text}");
        }
        return string.Join('\n', lines);
    }
}

/// <summary>A parsed act: its identity and its articles in text order, all versions included.</summary>
public sealed record Act(string Eli, string Title, DateOnly StateOfLaw, IReadOnlyList<Article> Articles)
{
    /// <summary>The version of an article in force on a date, if it exists then.</summary>
    public Article? Find(UnitNumber number, DateOnly date) =>
        Articles.FirstOrDefault(a => a.Number == number && a.Validity.IsInForceOn(date));
}
