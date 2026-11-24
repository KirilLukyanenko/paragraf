using Paragraf.Core.Answers;

namespace Paragraf.UnitTests;

public sealed class LanguageCheckTests
{
    [Theory]
    [InlineData("Ile dni urlopu przysługuje po 11 latach pracy?", "pl")]
    [InlineData("Pracodawca wypowiada umowę pracownikowi E-1003 10 grudnia 2026. Kiedy kończy się okres wypowiedzenia?", "pl")]
    [InlineData("We are giving notice to E-1003 on 10 December 2026. What notice period applies, and on what day does it end?", "en")]
    [InlineData("Скільки годин становить норма робочого часу на повну ставку в грудні 2026 року?", "uk")]
    [InlineData("Сколько дней отпуска мне положено, если я работаю 3 года?", "ru")]
    [InlineData("Para E-1003 el período de preaviso es de 3 meses y termina el 2027-03-31. Según el sistema de nómina, E-1003 trabaja desde 2019-09-01.", "es")]
    public void Detects_the_languages_the_service_meets(string text, string language) =>
        Assert.Equal(language, LanguageCheck.Detect(text));

    [Fact]
    public void An_English_answer_quoting_Polish_law_is_English()
    {
        var answer = "You get 26 days of leave, because the code says „26 dni – jeżeli pracownik jest zatrudniony co najmniej 10 lat” (art. 154 § 1). Years with previous employers count too.";
        Assert.Null(LanguageCheck.Mismatch("How many days of leave do I get after 11 years?", answer));
    }

    [Fact]
    public void A_Spanish_answer_to_an_English_question_is_a_mismatch()
    {
        var mismatch = LanguageCheck.Mismatch(
            "We are giving notice to E-1003 on 10 December 2026. What notice period applies, and on what day does it end?",
            "Para E-1003 el período de preaviso es de 3 meses y termina el 2027-03-31. Según el sistema de nómina, E-1003 trabaja desde 2019-09-01.");
        Assert.Equal(("en", "es"), mismatch);
    }

    [Fact]
    public void Every_eval_question_is_detected_as_its_language()
    {
        var files = new[] { "evals/questions.jsonl", "evals/holdout.jsonl", "evals/payroll/questions.jsonl" };
        var wrong = files
            .SelectMany(f => File.ReadLines(Path.Combine(KodeksPracy.RepoRoot(), f)))
            .Where(l => l.Trim().Length > 0)
            .Select(l => System.Text.Json.JsonDocument.Parse(l).RootElement)
            .Select(q => (Id: q.GetProperty("id").GetString(), Lang: q.GetProperty("lang").GetString(), Text: q.GetProperty("question").GetString()!))
            .Where(q => LanguageCheck.Detect(q.Text) != q.Lang)
            .Select(q => $"{q.Id} ({q.Lang} → {LanguageCheck.Detect(q.Text) ?? "?"}): {q.Text}")
            .ToList();
        Assert.True(wrong.Count == 0, string.Join("\n", wrong));
    }

    /// <summary>Real answers from the kept eval runs: no false alarms, and the two answers in the wrong language are caught.</summary>
    [Fact]
    public void Real_answers_in_the_eval_runs_are_flagged_only_when_wrong()
    {
        var root = KodeksPracy.RepoRoot();
        var questions = new[] { "evals/questions.jsonl", "evals/payroll/questions.jsonl" }
            .SelectMany(f => File.ReadLines(Path.Combine(root, f)))
            .Where(l => l.Trim().Length > 0)
            .Select(l => System.Text.Json.JsonDocument.Parse(l).RootElement)
            .ToDictionary(q => q.GetProperty("id").GetString()!, q => q.GetProperty("question").GetString()!);
        var flagged = new List<string>();
        var answers = 0;
        foreach (var file in Directory.GetFiles(Path.Combine(root, "evals", "runs"), "*.json"))
        {
            var results = System.Text.Json.JsonDocument.Parse(File.ReadAllText(file)).RootElement.GetProperty("results");
            foreach (var r in results.EnumerateArray())
            {
                if (!r.TryGetProperty("answer", out var a) || a.ValueKind != System.Text.Json.JsonValueKind.String) continue;
                answers++;
                var id = r.GetProperty("id").GetString()!;
                if (LanguageCheck.Mismatch(questions[id], a.GetString()!) is { } m) flagged.Add($"{Path.GetFileNameWithoutExtension(file)} {id} {m}");
            }
        }
        Assert.True(answers > 400, $"only {answers} answers read");
        // Every one is real. Haiku answered Polish questions in English (q003 switches halfway). With the payroll
        // tools, whose results quote the Polish code, Sonnet answered two English questions in Polish and one in Spanish.
        Assert.Equal(
        [
            "answers-haiku-5-5-medium q003 (pl, en)", "answers-haiku-5-5-medium q008 (pl, en)", "answers-haiku-5-5-medium q064 (pl, en)",
            "payroll-sonnet-5-5-medium p02 (en, pl)", "payroll-sonnet-5-5-medium p04 (en, es)", "payroll-sonnet-5-5-medium p08 (en, pl)",
        ], flagged.Order());
    }

    [Fact]
    public void Short_or_unclear_text_is_not_judged() => Assert.Null(LanguageCheck.Detect("E-1001?"));
}
