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
    [property: JsonPropertyName("unsupported_claims"), Description("Statements of law or numbers in the answer that the sources don't support. Empty if none.")]
    IReadOnlyList<string> UnsupportedClaims);

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

        Then list unsupported claims: statements of law, periods, amounts or percentages in the answer that the
        sources do not support. Arithmetic done correctly from a rule in the sources is supported. General advice
        ("consult a lawyer") is not a claim. Be strict about numbers and conditions, lenient about wording.
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
