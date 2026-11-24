using System.Globalization;
using System.Text;
using System.Text.Json;
using Paragraf.Core.Answers;
using Paragraf.Rag;

namespace Paragraf.Cli.Evals;

/// <summary>
/// What the demo was asked (the API's ask log): how many asks, what they cost, how many were repeats, what visitors
/// marked, and the misses (marked unhelpful, or answered with a citation or amount that failed the check) as
/// unlabelled eval questions. Visitors' questions are not committed: the owner reads, anonymises and labels them.
/// </summary>
public static class AskReport
{
    /// <summary>Asks exported from the server, one JSON object per line (scripts/demo-asks.ps1).</summary>
    public static IReadOnlyList<AskLog.Entry> Load(string path) =>
        File.ReadLines(path).Where(l => l.Trim().Length > 0).Select(l =>
        {
            var r = JsonDocument.Parse(l).RootElement;
            string? Text(string name) => r.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null ? v.ToString() : null;
            return new AskLog.Entry(
                Guid.Parse(Text("id")!),
                DateTimeOffset.Parse(Text("at")!, CultureInfo.InvariantCulture),
                Text("question")!,
                DateOnly.Parse(Text("as_of")!, CultureInfo.InvariantCulture),
                Text("model")!,
                Text("version")!,
                Text("status")!,
                r.GetProperty("grounded").GetBoolean(),
                r.GetProperty("cacheable").GetBoolean(),
                r.GetProperty("cost_usd").GetDecimal(),
                Text("trace_id"),
                Text("cached_from") is { } c ? Guid.Parse(c) : null,
                r.TryGetProperty("helpful", out var h) && h.ValueKind != JsonValueKind.Null ? h.GetBoolean() : null,
                r.GetProperty("response").GetRawText());
        }).ToList();

    public static bool IsMiss(AskLog.Entry e) =>
        e.Helpful == false || (e.CachedFrom is null && e.Status == "answered" && !e.Grounded);

    public static string Summary(IReadOnlyList<AskLog.Entry> asks)
    {
        var s = new StringBuilder();
        if (asks.Count == 0) return "No asks in this period.";
        var calls = asks.Where(a => a.CachedFrom is null).ToList();
        var answered = calls.Where(a => a.Status == "answered").ToList();
        s.AppendLine($"{asks.Count} asks from {asks.Min(a => a.At):yyyy-MM-dd} to {asks.Max(a => a.At):yyyy-MM-dd}: " +
                     $"{calls.Count} model calls, {asks.Count - calls.Count} repeats answered from the log");
        s.AppendLine($"cost ${calls.Sum(a => a.CostUsd):0.000}, ${(calls.Count == 0 ? 0 : calls.Average(a => a.CostUsd)):0.0000} per call; " +
                     string.Join(", ", calls.GroupBy(a => a.Model).Select(g => $"{g.Key} {g.Count()}")));
        s.AppendLine("status: " + string.Join(", ", calls.GroupBy(a => a.Status).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
        if (answered.Count > 0) s.AppendLine($"grounded: {answered.Count(a => a.Grounded)} of {answered.Count} answered");
        s.AppendLine("languages: " + string.Join(", ", asks.GroupBy(a => LanguageCheck.Detect(a.Question) ?? "?")
            .OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
        s.AppendLine($"marked: {asks.Count(a => a.Helpful == true)} helpful, {asks.Count(a => a.Helpful == false)} not helpful, " +
                     $"{asks.Count(a => a.Helpful is null)} unmarked");
        var misses = Misses(asks);
        s.AppendLine();
        s.AppendLine(misses.Count == 0 ? "No misses." : $"{misses.Count} misses:");
        foreach (var m in misses)
        {
            var why = m.Helpful == false ? "marked not helpful" : "a check failed";
            s.AppendLine($"  {m.At:yyyy-MM-dd HH:mm} [{m.Status}{(m.Grounded ? "" : ", ungrounded")}] {why}: {m.Question}");
        }
        return s.ToString();
    }

    /// <summary>One per question (a repeat that was marked counts for the question it repeated).</summary>
    public static IReadOnlyList<AskLog.Entry> Misses(IReadOnlyList<AskLog.Entry> asks) =>
        asks.Where(IsMiss).GroupBy(a => AskLog.Key(a.Question)).Select(g => g.First()).ToList();

    /// <summary>The misses in the eval set's format, to label: expected units, evidence and answer points are empty.</summary>
    public static IEnumerable<string> Candidates(IReadOnlyList<AskLog.Entry> asks) =>
        Misses(asks).Select((m, i) =>
        {
            var answer = JsonDocument.Parse(m.Response).RootElement.GetProperty("answer").GetString();
            var why = m.Helpful == false ? "marked not helpful" : "a citation or amount failed the check";
            return JsonSerializer.Serialize(new
            {
                id = $"d{m.At:yyyyMMdd}-{i + 1:000}",
                lang = LanguageCheck.Detect(m.Question) ?? "?",
                category = "unlabelled",
                question = m.Question,
                expected = Array.Empty<ExpectedUnit>(),
                evidence = Array.Empty<string>(),
                answer_points = Array.Empty<string>(),
                notes = $"From the demo on {m.At:yyyy-MM-dd}, law as of {m.AsOf:yyyy-MM-dd}, {m.Model}: {m.Status}, {why}. It answered: {answer}",
            }, Readable);
        });

    // Polish and Ukrainian letters as they are, like the eval files.
    private static readonly JsonSerializerOptions Readable = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}
