using System.Text.RegularExpressions;

namespace Paragraf.Core.Law;

/// <summary>
/// Splits the text of a Dziennik Ustaw PDF (as <c>pdftotext -layout</c> writes it) into the body
/// lines and the footnotes. Pages are separated by form feeds; each page starts with a running
/// header and may end with a block of footnotes. Footnotes are numbered across the whole act, and
/// their markers are glued to the text they annotate: <c>Art. 67[20].22)</c>, <c>§ 2.5)</c>,
/// <c>Kodeks pracy1)</c>. A footnote block can't be told from a numbered list by its look alone
/// (both are <c>N) text</c> at the start of a line), so a line opens a footnote only if its number
/// is the next footnote number and that marker has already appeared in the text.
/// </summary>
public static partial class DziennikUstawText
{
    public sealed record Split(IReadOnlyList<string> BodyLines, IReadOnlyDictionary<int, string> Footnotes);

    /// <param name="text">The whole extracted text.</param>
    /// <param name="startMarker">Text on the first page that belongs to the act itself (the annex of the
    /// announcement); pages before it (the announcement) are skipped.</param>
    public static Split Read(string text, string startMarker)
    {
        var pages = text.Replace("\r\n", "\n").Split('\f');
        var first = Array.FindIndex(pages, p => p.Contains(startMarker, StringComparison.Ordinal));
        if (first < 0) throw new FormatException($"Start marker '{startMarker}' not found.");

        var body = new List<string>();
        var footnotes = new Dictionary<int, string>();
        var referenced = new HashSet<int>();
        var next = 1;

        foreach (var page in pages.Skip(first))
        {
            var lines = page.Split('\n');
            var start = page == pages[first] ? FirstLineOf(lines, startMarker) : 0;
            var current = -1;
            for (var i = start; i < lines.Length; i++)
            {
                var line = lines[i];
                if (RunningHeader().IsMatch(line)) continue;

                var opener = FootnoteOpener().Match(line);
                if (opener.Success && int.Parse(opener.Groups[1].Value) == next && referenced.Contains(next))
                {
                    current = next++;
                    footnotes[current] = opener.Groups[2].Value.Trim();
                    continue;
                }
                if (current >= 0)
                {
                    // Inside the footnote block every remaining line on the page belongs to a footnote.
                    if (line.Trim().Length > 0) footnotes[current] = JoinLine(footnotes[current], line.Trim());
                    continue;
                }
                foreach (Match marker in GluedMarker().Matches(line))
                {
                    referenced.Add(int.Parse(marker.Groups[1].Value));
                }
                body.Add(line);
            }
        }
        return new Split(body, footnotes);
    }

    private static int FirstLineOf(string[] lines, string marker) =>
        Math.Max(0, Array.FindIndex(lines, l => l.Contains(marker, StringComparison.Ordinal)));

    /// <summary>Joins a wrapped line, removing a hyphen that only splits a word across lines.</summary>
    public static string JoinLine(string text, string next)
    {
        if (text.Length == 0) return next;
        if (text.Length >= 2 && text[^1] == '-' && char.IsLetter(text[^2]) && next.Length > 0 && char.IsLower(next[0]))
        {
            return text[..^1] + next;
        }
        return text + " " + next;
    }

    [GeneratedRegex(@"^\s*Dziennik Ustaw\s+–\s*\d+\s*–\s+Poz\.\s*\d+\s*$")]
    private static partial Regex RunningHeader();

    /// <summary>A footnote starts at column 0: <c>12) W brzmieniu ustalonym przez …</c>.</summary>
    [GeneratedRegex(@"^(\d{1,3})\)\s+(.*)$")]
    private static partial Regex FootnoteOpener();

    /// <summary>A footnote marker glued to the preceding character: a letter, a period, or a closing bracket
    /// (<c>(uchylony)4)</c>, <c>6)14)</c>, <c>23[1]7)</c>).</summary>
    [GeneratedRegex(@"(?<=[\p{L}.\])])(\d{1,3})\)")]
    public static partial Regex GluedMarker();
}
