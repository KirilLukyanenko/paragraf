using System.Text.Json;
using System.Text.Json.Serialization;
using Paragraf.Rag;

namespace Paragraf.Cli.Evals;

public sealed record ExpectedUnit(
    [property: JsonPropertyName("article")] string Article,
    [property: JsonPropertyName("paragraph")] string? Paragraph);

public sealed record EvalQuestion(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("lang")] string Lang,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("expected")] IReadOnlyList<ExpectedUnit> Expected,
    [property: JsonPropertyName("evidence")] IReadOnlyList<string> Evidence,
    [property: JsonPropertyName("answer_points")] IReadOnlyList<string> AnswerPoints,
    [property: JsonPropertyName("notes")] string? Notes)
{
    public static IReadOnlyList<EvalQuestion> Load(string path) =>
        File.ReadLines(path).Where(l => l.Trim().Length > 0)
            .Select(l => JsonSerializer.Deserialize<EvalQuestion>(l) ?? throw new FormatException(l))
            .ToList();

    /// <summary>The statuses a correct answer may have, by category.</summary>
    public IReadOnlySet<AnswerStatus> AcceptableStatuses => Category switch
    {
        "not_in_code" => new HashSet<AnswerStatus> { AnswerStatus.NotInSources },
        "out_of_domain" => new HashSet<AnswerStatus> { AnswerStatus.OutOfScope },
        // A request to do something harmful may be declined outright: that is a correct answer too.
        "injection" when Expected.Count == 0 => new HashSet<AnswerStatus> { AnswerStatus.OutOfScope, AnswerStatus.NotInSources, AnswerStatus.Refused },
        _ => new HashSet<AnswerStatus> { AnswerStatus.Answered },
    };

    public bool Answerable => Expected.Count > 0;
}
