using System.Globalization;
using System.Text.RegularExpressions;

namespace Paragraf.Core.Answers;

/// <summary>An amount of money written in an answer, and whether a calculation or the question gave it.</summary>
public sealed record CheckedAmount(string Text, decimal Value, bool Verified);

/// <summary>
/// The model may not compute money: every amount in the answer must be one the tools returned or the user wrote.
/// Amounts are numbers next to a currency (zł, PLN, złotych, EUR, €), in Polish or English notation.
/// </summary>
public static partial class AmountCheck
{
    private const string Number = @"\d{1,3}(?:[   ,.]\d{3})+(?:[.,]\d{1,2})?|\d+(?:[.,]\d{1,2})?";
    private const string Currency = @"zł|zl\b|PLN|złotych|złote|złoty|zlotych|EUR|€";

    [GeneratedRegex($@"(?<![\d.,])(?<n>{Number})\s?(?:{Currency})|(?:PLN|EUR|€)\s?(?<n>{Number})(?![\d])", RegexOptions.IgnoreCase)]
    private static partial Regex AmountPattern();

    public static IReadOnlyList<CheckedAmount> Check(string answer, IEnumerable<decimal> known)
    {
        var knownList = known.ToList();
        return Find(answer)
            .Select(a => new CheckedAmount(a.Text, a.Value, knownList.Any(k => Math.Abs(k - a.Value) < 0.005m)))
            .ToList();
    }

    public static IReadOnlyList<(string Text, decimal Value)> Find(string text) =>
        AmountPattern().Matches(text)
            .Select(m => (m.Value.Trim(), Parse(m.Groups["n"].Value)))
            .Where(a => a.Item2 is not null)
            .Select(a => (a.Item1, a.Item2!.Value))
            .ToList();

    /// <summary>Numbers anywhere in a text, in either notation: what the user may have quoted in the question.</summary>
    public static IEnumerable<decimal> NumbersIn(string text) =>
        Regex.Matches(text, Number).Select(m => Parse(m.Value)).Where(v => v is not null).Select(v => v!.Value);

    /// <summary>
    /// "12 800,00", "12,800.00", "12.800", "1234,5" and "1234.56". A separator followed by one or two digits at the
    /// end is the decimal point; any other separator groups thousands.
    /// </summary>
    public static decimal? Parse(string number)
    {
        var compact = new string(number.Where(c => !char.IsWhiteSpace(c) && c != ' ').ToArray());
        var last = compact.LastIndexOfAny(['.', ',']);
        string normalized;
        if (last >= 0 && compact.Length - last - 1 is 1 or 2)
        {
            normalized = compact[..last].Replace(".", "").Replace(",", "") + "." + compact[(last + 1)..];
        }
        else
        {
            normalized = compact.Replace(".", "").Replace(",", "");
        }
        return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) ? value : null;
    }
}
