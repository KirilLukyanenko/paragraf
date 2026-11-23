using System.Text.RegularExpressions;
using Paragraf.Core.Law;

namespace Paragraf.Core.Search;

/// <summary>An article (and maybe a paragraph) the user named in the question: "art. 151¹ § 2".</summary>
public sealed partial record ArticleReference(UnitNumber Article, UnitNumber? Paragraph)
{
    /// <summary>
    /// Finds article numbers written as people write them in Polish, English, Ukrainian or Russian:
    /// <c>art. 154</c>, <c>artykuł 36 § 1</c>, <c>article 151(1)</c>, <c>art. 18³ᵈ</c>, <c>ст. 154</c>, <c>статті 36</c>.
    /// </summary>
    public static IReadOnlyList<ArticleReference> Find(string question)
    {
        var found = new List<ArticleReference>();
        foreach (Match m in Pattern().Matches(question))
        {
            if (!UnitNumber.TryParse(m.Groups["art"].Value, out var article)) continue;
            UnitNumber? paragraph = UnitNumber.TryParse(m.Groups["par"].Value, out var p) ? p : null;
            var reference = new ArticleReference(article, paragraph);
            if (!found.Contains(reference)) found.Add(reference);
        }
        return found;
    }

    [GeneratedRegex(
        @"(?:\bart(?:ykuł\w*|icle|\.)?|\bст(?:атт\w*|ать\w*|\.)?)\s*(?<art>\d{1,3}(?:\^\w{1,3}|\[\w{1,3}\]|\(\d{1,2}[a-z]?\)|[⁰¹²³⁴⁵⁶⁷⁸⁹ᵃᵇᶜᵈᵉᶠᵍ]+)?)(?:\s*§\s*(?<par>\d{1,2}(?:\^\w{1,2}|[⁰¹²³⁴⁵⁶⁷⁸⁹]+)?))?",
        RegexOptions.IgnoreCase)]
    private static partial Regex Pattern();
}
