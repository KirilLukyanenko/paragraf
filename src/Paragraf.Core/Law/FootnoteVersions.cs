using System.Text.RegularExpressions;

namespace Paragraf.Core.Law;

/// <summary>
/// Reads the dates of versions from the footnotes of a consolidated text. Two kinds matter:
/// <list type="bullet">
/// <item><c>W tym brzmieniu obowiązuje do wejścia w życie zmiany, o której mowa w odnośniku 6.</c> — this
/// version ends when the amendment of footnote 6 comes into force.</item>
/// <item><c>W brzmieniu ustalonym przez art. 1 … ustawy …, która wejdzie w życie z dniem 5 listopada 2026 r.</c>
/// — this version starts on that date. A footnote can point to another one for the act and its date
/// (<c>ustawy, o której mowa w odnośniku 6</c>).</item>
/// </list>
/// Everything else (additions and changes already in force on the state of law) gives no dates:
/// the consolidated text is the law on its state-of-law date and stays so until a known change.
/// </summary>
public sealed partial class FootnoteVersions(IReadOnlyDictionary<int, string> footnotes, DateOnly stateOfLaw)
{
    public Validity ValidityOf(int? marker)
    {
        if (marker is not { } m || !footnotes.TryGetValue(m, out var text)) return Validity.Always;

        var endsWith = EndsWithAmendment().Match(text);
        if (endsWith.Success)
        {
            var until = EffectiveDate(int.Parse(endsWith.Groups[1].Value), depth: 0);
            return until is null ? Validity.Always : new Validity(null, until);
        }
        if (SetBy().IsMatch(text))
        {
            var from = EffectiveDate(m, depth: 0);
            // Only a change that starts after the state of law makes a separate future version.
            return from is { } f && f > stateOfLaw ? new Validity(f, null) : Validity.Always;
        }
        return Validity.Always;
    }

    /// <summary>The date the amendment named in a footnote comes into force, following references.</summary>
    public DateOnly? EffectiveDate(int marker, int depth)
    {
        if (depth > 5 || !footnotes.TryGetValue(marker, out var text)) return null;
        var date = ComesIntoForce().Match(text);
        if (date.Success && PolishDate.TryParse(date.Groups[1].Value, out var d)) return d;
        var reference = RefersTo().Match(text);
        return reference.Success ? EffectiveDate(int.Parse(reference.Groups[1].Value), depth + 1) : null;
    }

    [GeneratedRegex(@"W tym brzmieniu obowiązuje do wejścia w życie zmiany, o której mowa w odnośniku (\d+)")]
    private static partial Regex EndsWithAmendment();

    [GeneratedRegex(@"^(W brzmieniu ustalonym przez|Dodany przez|Dodana przez|Dodane przez)")]
    private static partial Regex SetBy();

    [GeneratedRegex(@"(?:wejdzie|weszła|wchodzi) w życie z dniem (\d{1,2} \p{L}+ \d{4})")]
    private static partial Regex ComesIntoForce();

    [GeneratedRegex(@"o której mowa w odnośniku (\d+)")]
    private static partial Regex RefersTo();
}
