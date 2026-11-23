using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Paragraf.Core.Search;

/// <summary>
/// Terms for the full-text half of the search. Polish inflects heavily (wynagrodzenie, wynagrodzenia,
/// wynagrodzeniu …) and PostgreSQL ships no Polish stemmer, so words are cut to their first
/// <see cref="PrefixLength"/> letters after removing diacritics: a crude stemmer, but the same on
/// both sides, and the eval set measures what it is worth against no stemming at all.
/// </summary>
public static partial class LexicalTerms
{
    public const int PrefixLength = 6;

    /// <summary>Very common words that only add noise to ranking.</summary>
    private static readonly HashSet<string> Stop = new(StringComparer.Ordinal)
    {
        "i", "w", "z", "na", "do", "o", "od", "po", "za", "u", "lub", "albo", "oraz", "a", "nie", "jest", "sie",
        "ze", "to", "tym", "tej", "ten", "ta", "jak", "jego", "jej", "ich", "przez", "przy", "dla", "ktory",
        "ktora", "ktore", "ktorym", "ktorej", "mowa", "ktorych", "art", "pkt", "lit", "co", "czy", "mi", "mnie",
        "moj", "moja", "moje", "jaki", "jaka", "jakie", "ile", "kiedy",
    };

    public static IReadOnlyList<string> Of(string text, int prefixLength = PrefixLength)
    {
        var terms = new List<string>();
        foreach (Match word in Word().Matches(RemoveDiacritics(text.ToLowerInvariant())))
        {
            var w = word.Value;
            if (w.Length < 3 && !char.IsAsciiDigit(w[0])) continue;
            if (Stop.Contains(w)) continue;
            terms.Add(w.Length > prefixLength ? w[..prefixLength] : w);
        }
        return terms;
    }

    /// <summary>The terms joined by spaces, as stored next to each chunk.</summary>
    public static string Document(string text, int prefixLength = PrefixLength) => string.Join(' ', Of(text, prefixLength));

    public static string RemoveDiacritics(string text)
    {
        // ł has no decomposition, so it needs its own rule.
        var decomposed = text.Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[\p{L}\d]+")]
    private static partial Regex Word();
}
