using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Paragraf.Core.Answers;
using Paragraf.Rag;

namespace Paragraf.Cli.Evals;

/// <summary>A question that needs a calculation, with the amounts and dates a correct answer states.</summary>
public sealed record PayrollQuestion(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("lang")] string Lang,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("expected_tools")] IReadOnlyList<string> ExpectedTools,
    [property: JsonPropertyName("expected_amounts")] IReadOnlyList<decimal> ExpectedAmounts,
    [property: JsonPropertyName("expected_text")] IReadOnlyList<IReadOnlyList<string>> ExpectedText,
    [property: JsonPropertyName("expected_articles")] IReadOnlyList<string> ExpectedArticles,
    [property: JsonPropertyName("notes")] string? Notes)
{
    public static IReadOnlyList<PayrollQuestion> Load(string path) =>
        File.ReadLines(path).Where(l => l.Trim().Length > 0)
            .Select(l => JsonSerializer.Deserialize<PayrollQuestion>(l) ?? throw new FormatException(l))
            .ToList();
}

public sealed record PayrollResult(
    string Id,
    string Category,
    string Question,
    string? Status,
    string? Answer,
    IReadOnlyList<string> ToolsCalled,
    bool ToolsOk,
    IReadOnlyList<decimal> MissingAmounts,
    IReadOnlyList<string> UnverifiedAmounts,
    IReadOnlyList<string> MissingText,
    IReadOnlyList<string> MissingArticles,
    bool CitationsVerified,
    decimal CostUsd,
    double Seconds,
    string? Error)
{
    public bool Correct => Error is null && ToolsOk && MissingAmounts.Count == 0 && UnverifiedAmounts.Count == 0 && MissingText.Count == 0;
}

public sealed record PayrollSummary(
    string Name,
    string Model,
    int Questions,
    double Correct,
    double ToolsCorrect,
    double AmountsCorrect,
    double AmountsGrounded,
    double CitationsVerified,
    double ArticlesCited,
    decimal CostUsd,
    decimal CostPerQuestion,
    double P50Seconds,
    int Errors);

/// <summary>
/// Scores answers that need the calculation tools and the payroll system. Everything is checked by code: which tools
/// ran, that the expected amounts and dates are in the answer, that every amount came from a tool or the question,
/// and that citations pass the citation check. No judge.
/// </summary>
public static class PayrollEval
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public static async Task<(PayrollSummary, IReadOnlyList<PayrollResult>)> RunAsync(
        ParagrafEngine engine, IReadOnlyList<PayrollQuestion> questions, ParagrafSettings settings, DateOnly asOf, string name,
        int parallelism, Action<string> log, CancellationToken ct)
    {
        var results = new PayrollResult[questions.Count];
        await Parallel.ForEachAsync(Enumerable.Range(0, questions.Count),
            new ParallelOptions { MaxDegreeOfParallelism = parallelism, CancellationToken = ct },
            async (i, token) =>
            {
                results[i] = await RunOneAsync(engine, questions[i], settings, asOf, token);
                var r = results[i];
                log($"{r.Id} {(r.Correct ? "ok " : "MISS")} tools [{string.Join(", ", r.ToolsCalled)}] ${r.CostUsd:0.000} {r.Seconds:0.0}s{(r.Error is null ? "" : " ERROR " + r.Error)}");
            });

        var n = Math.Max(1, results.Length);
        double Share(Func<PayrollResult, bool> f) => (double)results.Count(f) / n;
        var cost = results.Sum(r => r.CostUsd);
        var seconds = results.Select(r => r.Seconds).Order().ToList();
        var summary = new PayrollSummary(name, settings.Answer.Model, results.Length,
            Share(r => r.Correct),
            Share(r => r.ToolsOk),
            Share(r => r.Error is null && r.MissingAmounts.Count == 0 && r.MissingText.Count == 0),
            Share(r => r.Error is null && r.UnverifiedAmounts.Count == 0),
            Share(r => r.Error is null && r.CitationsVerified),
            Share(r => r.Error is null && r.MissingArticles.Count == 0),
            cost, cost / n, seconds.Count == 0 ? 0 : seconds[seconds.Count / 2], results.Count(r => r.Error is not null));
        return (summary, results);
    }

    private static async Task<PayrollResult> RunOneAsync(ParagrafEngine engine, PayrollQuestion q, ParagrafSettings settings, DateOnly asOf, CancellationToken ct)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var answer = await engine.AskAsync(q.Question, asOf, settings, ct);
            var called = answer.ToolCalls.Select(c => c.Failed ? c.Name + "!" : c.Name).ToList();
            // For a missing-data question the expected tool is the one that failed; otherwise it must have succeeded.
            var toolsOk = q.ExpectedTools.All(t => answer.ToolCalls.Any(c => c.Name == t && c.Failed == (q.Category == "missing_data")));
            var found = AmountCheck.Find(answer.Text).Select(a => a.Value).ToList();
            var missingAmounts = q.ExpectedAmounts.Where(e => !found.Any(f => Math.Abs(f - e) < 0.005m)).ToList();
            var unverified = answer.Amounts.Where(a => !a.Verified).Select(a => a.Text).ToList();
            if (q.Category == "missing_data") unverified.AddRange(answer.Amounts.Where(a => a.Verified).Select(a => a.Text + " (no amount expected)"));
            var text = answer.Text.ToLowerInvariant();
            var missingText = q.ExpectedText.Where(any => !any.Any(t => text.Contains(t.ToLowerInvariant()))).Select(any => any[0]).ToList();
            var cited = answer.Citations.Where(c => c.Verified)
                .Select(c => Paragraf.Core.Law.UnitNumber.TryParse(c.Citation.Article, out var u) ? u.ToString() : c.Citation.Article).ToHashSet();
            var missingArticles = q.ExpectedArticles.Where(a => !cited.Contains(a)).ToList();
            var cost = answer.CostUsd + Pricing.Cost(settings.RewriteModel, answer.Retrieval.RewriteUsage);
            return new PayrollResult(q.Id, q.Category, q.Question, answer.Status.ToString(), answer.Text, called, toolsOk,
                missingAmounts, unverified, missingText, missingArticles, answer.Citations.All(c => c.Verified), cost, watch.Elapsed.TotalSeconds, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new PayrollResult(q.Id, q.Category, q.Question, null, null, [], false, [], [], [], [], false, 0, watch.Elapsed.TotalSeconds,
                $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public static async Task WriteAsync(string directory, PayrollSummary summary, IReadOnlyList<PayrollResult> results)
    {
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, summary.Name + ".json"), JsonSerializer.Serialize(new { summary, results }, Json));
        await File.WriteAllTextAsync(Path.Combine(directory, summary.Name + ".md"), Markdown(summary, results));
    }

    public static string Markdown(PayrollSummary s, IReadOnlyList<PayrollResult> results)
    {
        static string P(double v) => (v * 100).ToString("0", CultureInfo.InvariantCulture) + "%";
        var sb = new StringBuilder();
        sb.AppendLine($"# Payroll eval {s.Name}");
        sb.AppendLine();
        sb.AppendLine($"Answer model {s.Model}, {s.Questions} questions, ${s.CostUsd:0.00} (${s.CostPerQuestion:0.000} a question), p50 {s.P50Seconds:0.0} s.");
        sb.AppendLine();
        sb.AppendLine("| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited |");
        sb.AppendLine("|---|---|---|---|---|---|");
        sb.AppendLine($"| {P(s.Correct)} | {P(s.ToolsCorrect)} | {P(s.AmountsCorrect)} | {P(s.AmountsGrounded)} | {P(s.CitationsVerified)} | {P(s.ArticlesCited)} |");
        sb.AppendLine();
        sb.AppendLine("| Id | Category | Result | Tools | Problems |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var r in results)
        {
            var problems = new List<string>();
            if (r.Error is not null) problems.Add("error: " + r.Error);
            if (!r.ToolsOk) problems.Add("wrong tools");
            if (r.MissingAmounts.Count > 0) problems.Add("missing " + string.Join(", ", r.MissingAmounts));
            if (r.UnverifiedAmounts.Count > 0) problems.Add("unverified " + string.Join(", ", r.UnverifiedAmounts));
            if (r.MissingText.Count > 0) problems.Add("missing " + string.Join(", ", r.MissingText));
            if (r.MissingArticles.Count > 0) problems.Add("not cited: art. " + string.Join(", ", r.MissingArticles));
            if (!r.CitationsVerified) problems.Add("a citation failed the check");
            sb.AppendLine($"| {r.Id} | {r.Category} | {(r.Correct ? "correct" : "**miss**")} | {string.Join(", ", r.ToolsCalled)} | {string.Join("; ", problems)} |");
        }
        sb.AppendLine();
        sb.AppendLine("## Answers");
        foreach (var r in results)
        {
            sb.AppendLine();
            sb.AppendLine($"### {r.Id} ({r.Category})");
            sb.AppendLine();
            sb.AppendLine($"> {r.Question}");
            sb.AppendLine();
            sb.AppendLine(r.Answer ?? r.Error);
        }
        return sb.ToString();
    }
}
