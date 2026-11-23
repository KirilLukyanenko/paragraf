using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Paragraf.Rag;

namespace Paragraf.Cli.Evals;

[Description("Grades one answer against the points a correct answer must make.")]
public sealed record Verdict(
    [property: JsonPropertyName("points"), Description("One entry per expected point, in the given order.")]
    IReadOnlyList<PointVerdict> Points,
    [property: JsonPropertyName("contradicted_claims"), Description("Statements in the answer that the sources contradict: a wrong number, period, condition or rule. Empty if none.")]
    IReadOnlyList<string> ContradictedClaims,
    [property: JsonPropertyName("outside_claims"), Description("Statements of law in the answer that the sources neither support nor contradict (added from outside knowledge). Empty if none.")]
    IReadOnlyList<string> OutsideClaims);

public sealed record PointVerdict(
    [property: JsonPropertyName("met")] bool Met,
    [property: JsonPropertyName("reason"), Description("One short sentence.")] string Reason);

/// <summary>
/// The model-graded part of the eval: does the answer make the expected points, and does it claim
/// anything the sources don't say. Everything that code can check (status, citations, retrieval)
/// is checked by code instead; the judge only reads meaning.
/// </summary>
public sealed class Judge(IChatClient chat, string model)
{
    public string Model => model;

    private const string Instructions = """
        You grade answers of a question-answering system about the Polish Labour Code. You get the question,
        the sources the system was given, the system's answer, and the points a correct answer must make.

        For each point, decide whether the answer makes it (same meaning; any language; a number must match).
        A point about behaviour ("says the code does not regulate X", "does not invent a percentage") is met when
        the answer behaves that way.

        Then check the answer's statements of law, periods, amounts and percentages against the sources:
        - contradicted_claims: the sources say otherwise (a wrong number, period, condition or rule).
        - outside_claims: the sources neither say nor contradict it; it comes from outside knowledge.
        A statement that follows from the sources by plain reading or by correct arithmetic is supported, and so is
        a fair summary in other words. General advice ("check your contract", "consult a lawyer") and statements
        that the sources do not settle a point are not claims. Be strict about numbers and conditions, lenient
        about wording.
        """;

    public async Task<(Verdict? Verdict, UsageDetails? Usage)> GradeAsync(EvalQuestion q, Answer answer, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<question>").AppendLine(q.Question).AppendLine("</question>");
        sb.AppendLine("<sources>");
        foreach (var a in answer.Retrieval.Articles) sb.AppendLine(a.Article.Render(answer.AsOf)).AppendLine();
        sb.AppendLine("</sources>");
        sb.AppendLine($"<answer status=\"{answer.Status}\">").AppendLine(answer.Text).AppendLine("</answer>");
        sb.AppendLine("<expected_points>");
        for (var i = 0; i < q.AnswerPoints.Count; i++) sb.AppendLine($"{i + 1}. {q.AnswerPoints[i]}");
        sb.AppendLine("</expected_points>");

        var response = await chat.GetResponseAsync<Verdict>(
            [new ChatMessage(ChatRole.System, Instructions), new ChatMessage(ChatRole.User, sb.ToString())],
            new ChatOptions { ModelId = model, MaxOutputTokens = 4000, Reasoning = new ReasoningOptions { Effort = ReasoningEffort.Low } },
            cancellationToken: ct);
        return response.TryGetResult(out var verdict) && verdict.Points.Count == q.AnswerPoints.Count
            ? (verdict, response.Usage)
            : (null, response.Usage);
    }
}
