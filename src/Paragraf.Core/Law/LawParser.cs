using System.Text;
using System.Text.RegularExpressions;

namespace Paragraf.Core.Law;

/// <summary>
/// Parses the body of a Polish act (after <see cref="DziennikUstawText"/> removed page headers and
/// footnotes) into articles and paragraphs, with the headings (Dział, Rozdział, Oddział) each article
/// sits under and the dates of versions read from the footnotes.
/// </summary>
public static partial class LawParser
{
    public static Act Parse(string pdfText, string eli, string title, DateOnly stateOfLaw, string bodyStartsAt = "DZIAŁ PIERWSZY")
    {
        var split = DziennikUstawText.Read(pdfText, "Załącznik do obwieszczenia");
        var versions = new FootnoteVersions(split.Footnotes, stateOfLaw);
        var lines = split.BodyLines;
        var start = lines.ToList().FindIndex(l => l.Trim() == bodyStartsAt);
        if (start < 0) throw new FormatException($"Body start '{bodyStartsAt}' not found.");

        var builder = new Builder(versions, split.Footnotes.Keys.ToHashSet());
        for (var i = start; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            if (trimmed.Length == 0) { builder.Blank(); continue; }

            if (HeadingPattern().Match(trimmed) is { Success: true } heading)
            {
                builder.StartHeading(heading.Groups["level"].Value, trimmed);
                continue;
            }
            if (ArticleRange().IsMatch(line))
            {
                // "Art. 266–280." — a range of articles removed from the code; nothing to keep.
                builder.SkipRange();
                continue;
            }
            if (ArticleStart().Match(line) is { Success: true } article
                && UnitNumber.TryParse(Number(article), out var articleNumber)
                && builder.AcceptsArticle(articleNumber))
            {
                builder.StartArticle(articleNumber, Marker(article));
                var rest = article.Groups["rest"].Value;
                if (ParagraphStart().Match(rest) is { Success: true } inline && UnitNumber.TryParse(Number(inline), out var first))
                {
                    builder.StartParagraph(first, Marker(inline));
                    rest = inline.Groups["rest"].Value;
                }
                builder.Text(rest);
                continue;
            }
            if (ParagraphStart().Match(trimmed) is { Success: true } paragraph
                && UnitNumber.TryParse(Number(paragraph), out var paragraphNumber)
                && builder.AcceptsParagraph(paragraphNumber))
            {
                builder.StartParagraph(paragraphNumber, Marker(paragraph));
                builder.Text(paragraph.Groups["rest"].Value);
                continue;
            }
            builder.Text(trimmed);
        }
        return new Act(eli, title, stateOfLaw, builder.Finish());
    }

    private static string Number(Match m) =>
        m.Groups["sup"].Success ? $"{m.Groups["main"].Value}^{m.Groups["sup"].Value}" : m.Groups["main"].Value;

    private static int? Marker(Match m) => m.Groups["fn"].Success ? int.Parse(m.Groups["fn"].Value) : null;

    /// <summary><c>Art. 151[1].41) § 1. Za pracę …</c></summary>
    [GeneratedRegex(@"^\s*Art\.\s(?<main>\d{1,4})(?:\[(?<sup>[0-9]{1,2}[a-z]{0,2})\])?\.(?:(?<fn>\d{1,3})\))?\s*(?<rest>.*)$")]
    private static partial Regex ArticleStart();

    /// <summary><c>§ 2[1].5) Pracodawca …</c></summary>
    [GeneratedRegex(@"^\s*§\s(?<main>\d{1,3})(?:\[(?<sup>[0-9]{1,2}[a-z]{0,2})\])?\.(?:(?<fn>\d{1,3})\))?\s*(?<rest>.*)$")]
    private static partial Regex ParagraphStart();

    [GeneratedRegex(@"^\s*Art\.\s\d{1,4}\s*[–-]\s*\d{1,4}\.")]
    private static partial Regex ArticleRange();

    [GeneratedRegex(@"(godzin\p{L}*\s+)([01]?\d|2[0-4])00\b")]
    private static partial Regex HourAfterGodzina();

    [GeneratedRegex(@"(\d{1,2}:00\s+a\s+)([01]?\d|2[0-4])00\b")]
    private static partial Regex SecondHour();

    [GeneratedRegex(@"(\d{1,4})\[([0-9]{1,2}[a-z]{0,2})\]")]
    private static partial Regex BracketSuperscript();

    [GeneratedRegex(@"^(?<level>DZIAŁ|Rozdział|Oddział)\s+[\p{L}\d]+$")]
    private static partial Regex HeadingPattern();

    /// <summary>A line that starts an item of a list: <c>1)</c>, <c>2a)</c>, <c>b)</c>, or a dash.</summary>
    [GeneratedRegex(@"^(?:\d{1,2}[a-z]?\)(?:\d{1,3}\))?|[a-z]{1,2}\)|–)\s")]
    private static partial Regex ListItem();

    private sealed class Builder(FootnoteVersions versions, HashSet<int> footnoteNumbers)
    {
        private readonly List<Article> _articles = [];
        private readonly string?[] _headings = new string?[3];
        private int _headingLevel = -1;      // the heading whose title lines are being read
        private bool _headingHasTitle;

        private UnitNumber? _article;
        private int? _articleMarker;
        private readonly List<Paragraph> _paragraphs = [];
        private string _lead = "";
        private UnitNumber? _paragraph;
        private int? _paragraphMarker;
        private readonly StringBuilder _text = new();
        private string[] _articleHeadings = [];

        public bool AcceptsArticle(UnitNumber number)
        {
            // Text order: a new article never goes backwards. The same number again is a second version.
            var last = _articles.Count > 0 ? _articles[^1].Number : (UnitNumber?)null;
            var current = _article ?? last;
            return current is null || number.CompareTo(current.Value) >= 0;
        }

        public bool AcceptsParagraph(UnitNumber number)
        {
            if (_article is null) return false;
            var last = _paragraph ?? (_paragraphs.Count > 0 ? _paragraphs[^1].Number : null);
            return last is null ? number.Main == 1 || number.Main == 0 : number.CompareTo(last.Value) >= 0;
        }

        public void Blank()
        {
            if (_headingLevel >= 0 && _headingHasTitle) _headingLevel = -1;
        }

        public void StartHeading(string level, string label)
        {
            FlushArticle();
            var index = level switch { "DZIAŁ" => 0, "Rozdział" => 1, _ => 2 };
            _headings[index] = label;
            for (var i = index + 1; i < _headings.Length; i++) _headings[i] = null;
            _headingLevel = index;
            _headingHasTitle = false;
        }

        public void StartArticle(UnitNumber number, int? marker)
        {
            FlushArticle();
            _headingLevel = -1;
            _article = number;
            _articleMarker = marker;
            _articleHeadings = _headings.Where(h => h is not null).Select(h => h!).ToArray();
        }

        public void SkipRange()
        {
            FlushArticle();
            _headingLevel = -1;
        }

        public void StartParagraph(UnitNumber number, int? marker)
        {
            FlushParagraph();
            _paragraph = number;
            _paragraphMarker = marker;
        }

        public void Text(string line)
        {
            if (line.Length == 0) return;
            if (_headingLevel >= 0)
            {
                // Title lines of a heading: "Rozdział I" then "Przepisy wstępne".
                var label = _headings[_headingLevel]!;
                _headings[_headingLevel] = _headingHasTitle ? DziennikUstawText.JoinLine(label, line) : $"{label} – {line}";
                _headingHasTitle = true;
                return;
            }
            if (_article is null) return; // text before the first article (a repealed preamble)
            if (_text.Length > 0 && ListItem().IsMatch(line))
            {
                _text.Append('\n').Append(line);
                return;
            }
            var joined = DziennikUstawText.JoinLine(LastLine(), line);
            ReplaceLastLine(joined);
        }

        private string LastLine()
        {
            var s = _text.ToString();
            var nl = s.LastIndexOf('\n');
            return nl < 0 ? s : s[(nl + 1)..];
        }

        private void ReplaceLastLine(string line)
        {
            var s = _text.ToString();
            var nl = s.LastIndexOf('\n');
            _text.Clear();
            if (nl >= 0) _text.Append(s, 0, nl + 1);
            _text.Append(line);
        }

        private string TakeText()
        {
            var text = StripMarkers(_text.ToString().Trim());
            _text.Clear();
            return text;
        }

        /// <summary>Removes footnote markers left inside the text (<c>pracy1)</c> → <c>pracy</c>).</summary>
        private string StripMarkers(string text)
        {
            var stripped = DziennikUstawText.GluedMarker().Replace(text,
                m => footnoteNumbers.Contains(int.Parse(m.Groups[1].Value)) ? "" : m.Value);
            // References to other units keep the PDF's bracket form "art. 23[1]"; show them as people write them.
            stripped = BracketSuperscript().Replace(stripped,
                m => new UnitNumber(int.Parse(m.Groups[1].Value), m.Groups[2].Value).ToDisplay());
            // Hours are printed with superscript minutes (21⁰⁰), which the extraction flattens to "2100".
            stripped = HourAfterGodzina().Replace(stripped, "$1$2:00");
            return SecondHour().Replace(stripped, "$1$2:00");
        }

        private void FlushParagraph()
        {
            if (_article is null) return;
            var text = TakeText();
            if (_paragraph is { } number)
            {
                _paragraphs.Add(new Paragraph(number, text, IsRepealed(text), versions.ValidityOf(_paragraphMarker)));
            }
            else
            {
                _lead = text;
            }
            _paragraph = null;
            _paragraphMarker = null;
        }

        private void FlushArticle()
        {
            if (_article is not { } number) return;
            FlushParagraph();
            var repealed = _paragraphs.Count == 0 && IsRepealed(_lead);
            _articles.Add(new Article(number, _lead, _paragraphs.ToList(), _articleHeadings, repealed,
                versions.ValidityOf(_articleMarker)));
            _article = null;
            _articleMarker = null;
            _paragraphs.Clear();
            _lead = "";
        }

        private static bool IsRepealed(string text) =>
            text is "(uchylony)" or "(uchylona)" or "(uchylone)" or "(utracił moc)" or "(utraciła moc)" or "(utraciły moc)";

        public IReadOnlyList<Article> Finish()
        {
            FlushArticle();
            return _articles;
        }
    }
}
