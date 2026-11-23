using System.Text.RegularExpressions;

namespace Paragraf.Core.Law;

/// <summary>
/// The number of an article or a paragraph: a main number and an optional superscript,
/// as in art. 151¹ or art. 18³ᵈ. The PDF text writes the superscript in brackets (<c>151[1]</c>);
/// keys and the eval set write it with a caret (<c>151^1</c>); people write <c>151¹</c> or <c>151(1)</c>.
/// </summary>
public readonly partial record struct UnitNumber(int Main, string? Sup) : IComparable<UnitNumber>
{
    public override string ToString() => Sup is null ? Main.ToString() : $"{Main}^{Sup}";

    /// <summary>Human form with Unicode superscripts where they exist: 151¹, 18³ᵈ.</summary>
    public string ToDisplay() => Sup is null ? Main.ToString() : Main + ToSuperscript(Sup);

    public int CompareTo(UnitNumber other)
    {
        var byMain = Main.CompareTo(other.Main);
        if (byMain != 0) return byMain;
        if (Sup == other.Sup) return 0;
        if (Sup is null) return -1;
        if (other.Sup is null) return 1;
        return CompareSup(Sup, other.Sup);
    }

    /// <summary>Superscripts order as 1, 1a, 2, 3, 3a, 3b, 3ca, 3d … 10: number first, then letters.</summary>
    private static int CompareSup(string a, string b)
    {
        var (na, ra) = SplitSup(a);
        var (nb, rb) = SplitSup(b);
        var byNumber = na.CompareTo(nb);
        return byNumber != 0 ? byNumber : string.CompareOrdinal(ra, rb);
    }

    private static (int Number, string Letters) SplitSup(string sup)
    {
        var digits = 0;
        while (digits < sup.Length && char.IsAsciiDigit(sup[digits])) digits++;
        return (digits == 0 ? 0 : int.Parse(sup.AsSpan(0, digits)), sup[digits..]);
    }

    /// <summary>Parses <c>151</c>, <c>151^1</c>, <c>151[1]</c>, <c>151(1)</c>, <c>151¹</c>, <c>18^3ca</c>.</summary>
    public static bool TryParse(string? text, out UnitNumber number)
    {
        number = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var normalized = FromSuperscript(text.Trim());
        var match = NumberPattern().Match(normalized);
        if (!match.Success) return false;
        var sup = match.Groups["sup"].Success ? match.Groups["sup"].Value.ToLowerInvariant() : null;
        number = new UnitNumber(int.Parse(match.Groups["main"].Value), sup);
        return true;
    }

    public static UnitNumber Parse(string text) =>
        TryParse(text, out var number) ? number : throw new FormatException($"Not an article or paragraph number: '{text}'");

    [GeneratedRegex(@"^(?<main>\d{1,4})(?:(?:\^|\[|\()(?<sup>[0-9]{1,2}[a-z]{0,2})[\])]?)?$", RegexOptions.IgnoreCase)]
    private static partial Regex NumberPattern();

    private const string SuperDigits = "⁰¹²³⁴⁵⁶⁷⁸⁹";
    private static readonly Dictionary<char, char> SuperLetters = new()
    {
        ['a'] = 'ᵃ', ['b'] = 'ᵇ', ['c'] = 'ᶜ', ['d'] = 'ᵈ', ['e'] = 'ᵉ', ['f'] = 'ᶠ', ['g'] = 'ᵍ', ['h'] = 'ʰ',
    };

    private static string ToSuperscript(string sup) => string.Concat(sup.Select(c =>
        char.IsAsciiDigit(c) ? SuperDigits[c - '0'] : SuperLetters.GetValueOrDefault(c, c)));

    /// <summary>Turns trailing Unicode superscripts into the caret form: 151¹ → 151^1.</summary>
    private static string FromSuperscript(string text)
    {
        var firstSuper = -1;
        for (var i = 0; i < text.Length; i++)
        {
            if (SuperDigits.Contains(text[i]) || SuperLetters.ContainsValue(text[i])) { firstSuper = i; break; }
        }
        if (firstSuper <= 0) return text;
        var sup = string.Concat(text[firstSuper..].Select(c =>
        {
            var digit = SuperDigits.IndexOf(c);
            if (digit >= 0) return (char)('0' + digit);
            foreach (var (plain, super) in SuperLetters) if (super == c) return plain;
            return c;
        }));
        return text[..firstSuper] + "^" + sup;
    }
}
