using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Paragraf.Core.Law;
using Paragraf.Rag;

namespace Paragraf.Cli.Evals;

public sealed record EvalOptions
{
    public required string Name { get; init; }
    public required ParagrafSettings Settings { get; init; }
    /// <summary>Only retrieval: no answer and no judge. Costs the rewrite and the embeddings only.</summary>
    public bool RetrievalOnly { get; init; }
    public string JudgeModel { get; init; } = "claude-sonnet-5-5";
    public int Parallelism { get; init; } = 4;
    /// <summary>The date the law is read on: fixed, so runs on different days compare.</summary>
    public DateOnly AsOf { get; init; } = new(2026, 11, 23);
}

public sealed record QuestionResult
{
    public required string Id { get; init; }
    public required string Category { get; init; }
    public required string Lang { get; init; }
    public required IReadOnlyList<string> Expected { get; init; }
    public required IReadOnlyList<string> Retrieved { get; init; }
    public double? RetrievalRecall { get; init; }
    public int? FirstExpectedRank { get; init; }
    public string? Status { get; init; }
    public bool? StatusOk { get; init; }
    public int Citations { get; init; }
    public int VerifiedCitations { get; init; }
    public IReadOnlyList<string> CitationProblems { get; init; } = [];
    public double? CitationRecall { get; init; }
    public bool? Grounded { get; init; }
    public int? PointsMet { get; init; }
    public int? Points { get; init; }
    public IReadOnlyList<string> UnsupportedClaims { get; init; } = [];
    public string? Answer { get; init; }
    public decimal CostUsd { get; init; }
    public double RetrievalMs { get; init; }
    public double GenerationMs { get; init; }
    public string? Error { get; init; }
}

public sealed record RunSummary
{
    public required string Name { get; init; }
    public required string Configuration { get; init; }
    public required DateTimeOffset At { get; init; }
    public required int Questions { get; init; }
    public double RetrievalRecall { get; init; }
    public double RetrievalAllFound { get; init; }
    public double Mrr { get; init; }
    public double? StatusAccuracy { get; init; }
    public double? CitationPrecision { get; init; }
    public double? CitationRecall { get; init; }
    public double? GroundedRate { get; init; }
    public double? PointsMetRate { get; init; }
    public double? AnswersWithUnsupportedClaims { get; init; }
    public decimal CostUsd { get; init; }
    public decimal CostPerQuestionUsd { get; init; }
    public double LatencyP50Ms { get; init; }
    public double LatencyP95Ms { get; init; }
    public int Errors { get; init; }
    public IReadOnlyDictionary<string, CategorySummary> ByCategory { get; init; } = new Dictionary<string, CategorySummary>();
}

public sealed record CategorySummary(int Questions, double? RetrievalRecall, double? StatusAccuracy, double? PointsMetRate, double? GroundedRate);

public sealed class EvalRunner(Retriever retriever, Answerer answerer, IChatClient chat)
{
    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<(RunSummary Summary, IReadOnlyList<QuestionResult> Results)> RunAsync(
        IReadOnlyList<EvalQuestion> questions, EvalOptions options, Action<string> log, CancellationToken ct)
    {
        var judge = new Judge(chat, options.JudgeModel);
        var results = new ConcurrentBag<QuestionResult>();
        var done = 0;
        await Parallel.ForEachAsync(questions, new ParallelOptions { MaxDegreeOfParallelism = options.Parallelism, CancellationToken = ct },
            async (q, token) =>
            {
                var result = await RunOneAsync(q, options, judge, token);
                results.Add(result);
                var n = Interlocked.Increment(ref done);
                log($"[{n}/{questions.Count}] {q.Id} {q.Category,-26} recall={Fmt(result.RetrievalRecall)} status={result.Status ?? "-"}{(result.StatusOk == false ? " (WRONG)" : "")} " +
                    $"cites={result.VerifiedCitations}/{result.Citations} points={result.PointsMet?.ToString() ?? "-"}/{result.Points?.ToString() ?? "-"} ${result.CostUsd:0.0000}{(result.Error is null ? "" : " ERROR " + result.Error)}");
            });
        var ordered = results.OrderBy(r => r.Id, StringComparer.Ordinal).ToList();
        return (Summarize(ordered, options), ordered);
    }

    private async Task<QuestionResult> RunOneAsync(EvalQuestion q, EvalOptions options, Judge judge, CancellationToken ct)
    {
        var expected = q.Expected.Select(e => UnitNumber.Parse(e.Article).ToString()).Distinct().ToList();
        try
        {
            var retrieval = await retriever.RetrieveAsync(q.Question, options.AsOf, options.Settings.Retrieval, ct);
            var retrieved = retrieval.Articles.Select(a => a.Article.Key).ToList();
            double? recall = expected.Count == 0 ? null : expected.Count(retrieved.Contains) / (double)expected.Count;
            int? firstRank = expected.Count == 0 ? null : retrieved.Select((k, i) => (k, i)).Where(x => expected.Contains(x.k)).Select(x => (int?)x.i + 1).FirstOrDefault();
            var cost = Pricing.Cost(options.Settings.RewriteModel, retrieval.RewriteUsage);
            var baseResult = new QuestionResult
            {
                Id = q.Id, Category = q.Category, Lang = q.Lang, Expected = expected, Retrieved = retrieved,
                RetrievalRecall = recall, FirstExpectedRank = firstRank, CostUsd = cost,
                RetrievalMs = retrieval.Elapsed.TotalMilliseconds,
            };
            if (options.RetrievalOnly) return baseResult;

            var answer = await answerer.AnswerAsync(q.Question, options.AsOf, retrieval, options.Settings.Answer, ct);
            cost += answer.CostUsd;
            var citedArticles = answer.Citations.Where(c => c.Verified)
                .Select(c => UnitNumber.TryParse(c.Citation.Article, out var n) ? n.ToString() : "").ToHashSet();
            var (verdict, judgeUsage) = q.AnswerPoints.Count == 0 ? (null, null) : await judge.GradeAsync(q, answer, ct);
            cost += Pricing.Cost(judge.Model, judgeUsage);
            return baseResult with
            {
                Status = answer.Status.ToString(),
                StatusOk = q.AcceptableStatuses.Contains(answer.Status),
                Citations = answer.Citations.Count,
                VerifiedCitations = answer.Citations.Count(c => c.Verified),
                CitationProblems = answer.Citations.Where(c => !c.Verified).Select(c => $"{c.Display ?? c.Citation.Article}: {c.Problem}").ToList(),
                CitationRecall = expected.Count == 0 ? null : expected.Count(citedArticles.Contains) / (double)expected.Count,
                Grounded = answer.Grounded,
                PointsMet = verdict?.Points.Count(p => p.Met),
                Points = verdict is null ? null : q.AnswerPoints.Count,
                UnsupportedClaims = verdict?.UnsupportedClaims ?? [],
                Answer = answer.Text,
                CostUsd = cost,
                GenerationMs = answer.GenerationTime.TotalMilliseconds,
            };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new QuestionResult
            {
                Id = q.Id, Category = q.Category, Lang = q.Lang, Expected = expected, Retrieved = [],
                Error = $"{e.GetType().Name}: {e.Message}",
            };
        }
    }

    private static RunSummary Summarize(IReadOnlyList<QuestionResult> r, EvalOptions options)
    {
        var ok = r.Where(x => x.Error is null).ToList();
        var answerable = ok.Where(x => x.RetrievalRecall is not null).ToList();
        var answered = ok.Where(x => x.Status is not null).ToList();
        var latencies = ok.Select(x => x.RetrievalMs + x.GenerationMs).Order().ToList();
        var cost = r.Sum(x => x.CostUsd);
        return new RunSummary
        {
            Name = options.Name,
            Configuration = Describe(options),
            At = DateTimeOffset.UtcNow,
            Questions = r.Count,
            RetrievalRecall = Avg(answerable.Select(x => x.RetrievalRecall!.Value)) ?? 0,
            RetrievalAllFound = Avg(answerable.Select(x => x.RetrievalRecall == 1.0 ? 1.0 : 0.0)) ?? 0,
            Mrr = Avg(answerable.Select(x => x.FirstExpectedRank is { } k ? 1.0 / k : 0.0)) ?? 0,
            StatusAccuracy = Avg(answered.Select(x => x.StatusOk == true ? 1.0 : 0.0)),
            CitationPrecision = answered.Sum(x => x.Citations) == 0 ? null : answered.Sum(x => x.VerifiedCitations) / (double)answered.Sum(x => x.Citations),
            CitationRecall = Avg(answered.Where(x => x.CitationRecall is not null).Select(x => x.CitationRecall!.Value)),
            GroundedRate = Avg(answered.Select(x => x.Grounded == true ? 1.0 : 0.0)),
            PointsMetRate = answered.Sum(x => x.Points ?? 0) == 0 ? null : answered.Sum(x => x.PointsMet ?? 0) / (double)answered.Sum(x => x.Points ?? 0),
            AnswersWithUnsupportedClaims = answered.Count == 0 ? null : answered.Count(x => x.UnsupportedClaims.Count > 0) / (double)answered.Count,
            CostUsd = Math.Round(cost, 4),
            CostPerQuestionUsd = r.Count == 0 ? 0 : Math.Round(cost / r.Count, 4),
            LatencyP50Ms = Percentile(latencies, 0.5),
            LatencyP95Ms = Percentile(latencies, 0.95),
            Errors = r.Count - ok.Count,
            ByCategory = ok.GroupBy(x => x.Category).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => new CategorySummary(
                g.Count(),
                Avg(g.Where(x => x.RetrievalRecall is not null).Select(x => x.RetrievalRecall!.Value)),
                Avg(g.Where(x => x.StatusOk is not null).Select(x => x.StatusOk == true ? 1.0 : 0.0)),
                g.Sum(x => x.Points ?? 0) == 0 ? null : g.Sum(x => x.PointsMet ?? 0) / (double)g.Sum(x => x.Points ?? 0),
                Avg(g.Where(x => x.Grounded is not null).Select(x => x.Grounded == true ? 1.0 : 0.0)))),
        };
    }

    public static string Describe(EvalOptions o) => o.RetrievalOnly
        ? $"retrieval {o.Settings.Retrieval.Describe()}, rewrite {o.Settings.RewriteModel}, embeddings {o.Settings.EmbeddingModel}"
        : $"retrieval {o.Settings.Retrieval.Describe()}, answer {o.Settings.Answer.Model} effort {o.Settings.Answer.Effort}, judge {o.JudgeModel}";

    private static double? Avg(IEnumerable<double> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? null : list.Average();
    }

    private static double Percentile(IReadOnlyList<double> sorted, double p) =>
        sorted.Count == 0 ? 0 : sorted[Math.Min(sorted.Count - 1, (int)Math.Ceiling(p * sorted.Count) - 1)];

    private static string Fmt(double? v) => v is null ? "-" : v.Value.ToString("0.00");

    public static async Task WriteAsync(string directory, RunSummary summary, IReadOnlyList<QuestionResult> results)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{summary.Name}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new { summary, results }, Json));
        await File.WriteAllTextAsync(Path.Combine(directory, $"{summary.Name}.md"), Markdown(summary, results));
    }

    public static string Markdown(RunSummary s, IReadOnlyList<QuestionResult> results)
    {
        static string P(double? v) => v is null ? "–" : $"{v.Value * 100:0.0}%";
        var sb = new StringBuilder();
        sb.AppendLine($"# Eval run `{s.Name}`").AppendLine();
        sb.AppendLine($"{s.Configuration}. {s.Questions} questions, {s.At:yyyy-MM-dd HH:mm} UTC.").AppendLine();
        sb.AppendLine("| Metric | Value |").AppendLine("|---|---|");
        sb.AppendLine($"| Retrieval recall (expected articles among those given to the model) | {P(s.RetrievalRecall)} |");
        sb.AppendLine($"| All expected articles retrieved | {P(s.RetrievalAllFound)} |");
        sb.AppendLine($"| MRR of the first expected article | {s.Mrr:0.000} |");
        if (s.StatusAccuracy is not null)
        {
            sb.AppendLine($"| Status correct (answers / says not in the code / out of scope) | {P(s.StatusAccuracy)} |");
            sb.AppendLine($"| Citation precision (citations verified by code) | {P(s.CitationPrecision)} |");
            sb.AppendLine($"| Citation recall (expected articles cited and verified) | {P(s.CitationRecall)} |");
            sb.AppendLine($"| Grounded answers (every citation verified, at least one) | {P(s.GroundedRate)} |");
            sb.AppendLine($"| Expected points made (judge) | {P(s.PointsMetRate)} |");
            sb.AppendLine($"| Answers with an unsupported claim (judge) | {P(s.AnswersWithUnsupportedClaims)} |");
        }
        sb.AppendLine($"| Cost | ${s.CostUsd:0.00} (${s.CostPerQuestionUsd:0.0000} per question) |");
        sb.AppendLine($"| Latency p50 / p95 | {s.LatencyP50Ms / 1000:0.0} s / {s.LatencyP95Ms / 1000:0.0} s |");
        sb.AppendLine($"| Errors | {s.Errors} |").AppendLine();
        sb.AppendLine("| Category | n | Retrieval recall | Status | Points | Grounded |").AppendLine("|---|---|---|---|---|---|");
        foreach (var (category, c) in s.ByCategory)
        {
            sb.AppendLine($"| {category} | {c.Questions} | {P(c.RetrievalRecall)} | {P(c.StatusAccuracy)} | {P(c.PointsMetRate)} | {P(c.GroundedRate)} |");
        }
        var misses = results.Where(r => r.RetrievalRecall is < 1.0 || r.StatusOk == false || r.Grounded == false || r.UnsupportedClaims.Count > 0 || r.Error is not null).ToList();
        if (misses.Count > 0)
        {
            sb.AppendLine().AppendLine("## Misses").AppendLine();
            foreach (var r in misses)
            {
                var why = new List<string>();
                if (r.Error is not null) why.Add($"error: {r.Error}");
                if (r.RetrievalRecall is < 1.0) why.Add($"retrieved {string.Join(", ", r.Retrieved)}; expected {string.Join(", ", r.Expected)}");
                if (r.StatusOk == false) why.Add($"status {r.Status}");
                if (r.Grounded == false) why.Add($"citations: {string.Join("; ", r.CitationProblems.DefaultIfEmpty("none given"))}");
                if (r.UnsupportedClaims.Count > 0) why.Add($"unsupported: {string.Join(" | ", r.UnsupportedClaims)}");
                sb.AppendLine($"- **{r.Id}** ({r.Category}, {r.Lang}): {string.Join(". ", why)}");
            }
        }
        return sb.ToString();
    }
}
