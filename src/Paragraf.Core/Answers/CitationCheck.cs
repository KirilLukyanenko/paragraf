using System.Text;
using System.Text.RegularExpressions;
using Paragraf.Core.Law;

namespace Paragraf.Core.Answers;

/// <summary>A citation as the model gave it: the unit and the words it relies on.</summary>
public sealed record Citation(string Article, string? Paragraph, string Quote);

public enum CitationProblem
{
    None,
    /// <summary>The number isn't an article or paragraph number.</summary>
    Malformed,
    /// <summary>The article wasn't among the sources the model was given.</summary>
    NotInSources,
    /// <summary>The article has no such paragraph on the date.</summary>
    NoSuchParagraph,
    /// <summary>The quoted words are not in the cited unit.</summary>
    QuoteNotFound,
}

public sealed record CheckedCitation(Citation Citation, CitationProblem Problem, string? Display)
{
    public bool Verified => Problem == CitationProblem.None;
}

/// <summary>
/// Checks citations by code, not by another model: the article must be one the model was shown, the
/// paragraph must exist in the version in force on the date, and the quote must be the law's own
/// words (spacing, case, dashes and superscript spelling aside; "…" may skip words between parts).
/// </summary>
public static partial class CitationCheck
{
    public static IReadOnlyList<CheckedCitation> Check(Act act, DateOnly date, IReadOnlySet<string> sourceArticles, IEnumerable<Citation> citations) =>
        citations.Select(c => CheckOne(act, date, sourceArticles, c)).ToList();

    private static CheckedCitation CheckOne(Act act, DateOnly date, IReadOnlySet<string> sourceArticles, Citation citation)
    {
        if (!UnitNumber.TryParse(citation.Article, out var articleNumber)) return new(citation, CitationProblem.Malformed, null);
        UnitNumber? paragraphNumber = null;
        if (!string.IsNullOrWhiteSpace(citation.Paragraph))
        {
            if (!UnitNumber.TryParse(citation.Paragraph, out var p)) return new(citation, CitationProblem.Malformed, null);
            paragraphNumber = p;
        }
        var display = paragraphNumber is { } pn ? $"art. {articleNumber.ToDisplay()} § {pn.ToDisplay()}" : $"art. {articleNumber.ToDisplay()}";
        var article = act.Find(articleNumber, date);
        if (article is null || !sourceArticles.Contains(article.Key)) return new(citation, CitationProblem.NotInSources, display);

        string text;
        if (paragraphNumber is { } number)
        {
            var paragraph = article.ParagraphsOn(date).FirstOrDefault(p => p.Number == number);
            if (paragraph is null) return new(citation, CitationProblem.NoSuchParagraph, display);
            text = paragraph.Text;
        }
        else
        {
            text = article.Render(date);
        }
        return new(citation, QuoteIsIn(citation.Quote, text) ? CitationProblem.None : CitationProblem.QuoteNotFound, display);
    }

    /// <summary>True if every part of the quote (split at an ellipsis) is in the text, in order.</summary>
    public static bool QuoteIsIn(string quote, string text)
    {
        var haystack = Normalize(text);
        var parts = Ellipsis().Split(quote).Select(Normalize).Where(p => p.Length > 0).ToList();
        if (parts.Count == 0 || parts.Sum(p => p.Length) < 8) return false; // too short to prove anything
        var from = 0;
        foreach (var part in parts)
        {
            var at = haystack.IndexOf(part, from, StringComparison.Ordinal);
            if (at < 0) return false;
            from = at + part.Length;
        }
        return true;
    }

    public static string Normalize(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var c in text.ToLowerInvariant())
        {
            var mapped = c switch
            {
                '–' or '—' or '‑' or '−' => '-',
                '„' or '”' or '“' or '«' or '»' => '"',
                '’' or '‘' => '\'',
                _ => c,
            };
            sb.Append(mapped);
        }
        var s = Superscripts().Replace(sb.ToString(), m => "^" + FromSuperscript(m.Value));
        s = BracketSup().Replace(s, "^$1");
        s = s.Replace("§", " § ");
        s = Whitespace().Replace(s, " ").Trim();
        return s.Trim('"', '\'', ' ', '.', ',', ';', ':');
    }

    private static string FromSuperscript(string s)
    {
        const string digits = "⁰¹²³⁴⁵⁶⁷⁸⁹";
        const string letters = "ᵃᵇᶜᵈᵉᶠᵍ";
        return string.Concat(s.Select(c => digits.IndexOf(c) is var d and >= 0 ? (char)('0' + d) : (char)('a' + letters.IndexOf(c))));
    }

    [GeneratedRegex(@"\s*(?:…|\.\.\.|\[\.\.\.\]|\(\.\.\.\))\s*")]
    private static partial Regex Ellipsis();

    [GeneratedRegex("[⁰¹²³⁴⁵⁶⁷⁸⁹ᵃᵇᶜᵈᵉᶠᵍ]+")]
    private static partial Regex Superscripts();

    [GeneratedRegex(@"\[([0-9]{1,2}[a-z]{0,2})\]")]
    private static partial Regex BracketSup();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
