using System.Text.Json;
using Microsoft.Extensions.AI;
using Paragraf.Core.Law;
using Paragraf.Rag;

namespace Paragraf.IntegrationTests;

/// <summary>The answerer's checks around the model, with a scripted model instead of a real one.</summary>
public sealed class AnswererTests
{
    private static string RepoRoot([System.Runtime.CompilerServices.CallerFilePath] string file = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, "..", ".."));

    private static readonly LawLibrary Library = LawLibrary.KodeksPracy(Path.Combine(RepoRoot(), "data"));
    private static readonly DateOnly Today = new(2026, 11, 23);

    private static object Reply(string text) => new
    {
        status = "answered",
        answer = text,
        citations = new[] { new { article = "36", paragraph = "1", quote = "3 miesiące, jeżeli pracownik był zatrudniony co najmniej 3 lata" } },
    };

    private static Retrieval Sources() =>
        new([new RetrievedArticle(Library.Act.Find(UnitNumber.Parse("36"), Today)!, ["36§1"], 1)], [], null, TimeSpan.Zero, null);

    [Fact]
    public async Task An_answer_in_another_language_is_written_again_once()
    {
        var model = new ScriptedModel(
            Reply("El período de preaviso es de 3 meses, porque el empleado trabaja desde hace más de 3 años."),
            Reply("The notice period is 3 months, because the employee has worked there for more than 3 years."));
        var answer = await new Answerer(model, Library).AnswerAsync(
            "What notice period applies after 7 years with the employer?", Today, Sources(), new AnswerSettings(), ct: TestContext.Current.CancellationToken);

        Assert.Equal(2, model.Requests.Count);
        Assert.Contains("English", model.Requests[1].Last().Text);
        Assert.StartsWith("The notice period is 3 months", answer.Text);
        Assert.Equal(new LanguageOutcome("en", "en", true), answer.Language);
        Assert.True(answer.Grounded);
        Assert.Equal(2200, answer.Usage!.InputTokenCount + answer.Usage.OutputTokenCount); // both calls are paid for
    }

    [Fact]
    public async Task An_answer_in_the_right_language_is_sent_once_and_the_prompt_names_the_language()
    {
        var model = new ScriptedModel(Reply("The notice period is 3 months, because the employee has worked there for more than 3 years."));
        var answer = await new Answerer(model, Library).AnswerAsync(
            "What notice period applies after 7 years with the employer?", Today, Sources(), new AnswerSettings(), ct: TestContext.Current.CancellationToken);

        Assert.Single(model.Requests);
        Assert.Contains("<question language=\"English\">", model.Requests[0].Last().Text);
        Assert.False(answer.Language!.Rewritten);
    }

    [Fact]
    public async Task An_amount_the_model_worked_out_itself_makes_the_answer_ungrounded()
    {
        var model = new ScriptedModel(Reply("The notice period is 3 months, so you are owed 3 × 8 000 zł = 24 000 zł for it."));
        var answer = await new Answerer(model, Library).AnswerAsync(
            "I earn 8 000 zł and have worked 7 years. How much is my pay for the notice period?", Today, Sources(), new AnswerSettings(),
            ct: TestContext.Current.CancellationToken);

        Assert.Contains(answer.Amounts, a => a.Value == 8_000m && a.Verified);   // from the question
        Assert.Contains(answer.Amounts, a => a.Value == 24_000m && !a.Verified); // the model's own arithmetic
        Assert.False(answer.Grounded);
    }
}
