using System.Text.Json;
using Paragraf.Core.Answers;
using Paragraf.Core.Law;

namespace Paragraf.UnitTests;

public class CitationCheckTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);
    private static readonly HashSet<string> Sources = ["154", "151^1", "36", "18^3d"];

    private static CheckedCitation Check(string article, string? paragraph, string quote) =>
        CitationCheck.Check(KodeksPracy.Act, Today, Sources, [new Citation(article, paragraph, quote)]).Single();

    [Fact]
    public void Exact_words_from_the_cited_paragraph_are_verified() =>
        Assert.True(Check("154", "1", "20 dni – jeżeli pracownik jest zatrudniony krócej niż 10 lat").Verified);

    [Fact]
    public void Spacing_case_dashes_and_quotes_do_not_matter() =>
        Assert.True(Check("154", "1", "„20 DNI - jeżeli pracownik   jest zatrudniony krócej niż 10 lat”").Verified);

    [Fact]
    public void An_ellipsis_may_skip_words_but_the_parts_must_keep_their_order()
    {
        Assert.True(Check("151^1", "1", "dodatek w wysokości: … 50 % wynagrodzenia").Verified);
        Assert.Equal(CitationProblem.QuoteNotFound, Check("151^1", "1", "50 % wynagrodzenia … dodatek w wysokości").Problem);
    }

    [Fact]
    public void A_paraphrase_is_not_a_quote() =>
        Assert.Equal(CitationProblem.QuoteNotFound, Check("154", "1", "pracownik z krótszym stażem ma 20 dni urlopu").Problem);

    [Fact]
    public void A_quote_from_another_paragraph_is_not_verified() =>
        Assert.Equal(CitationProblem.QuoteNotFound, Check("154", "2", "20 dni – jeżeli pracownik jest zatrudniony krócej niż 10 lat").Problem);

    [Fact]
    public void An_article_the_model_was_not_shown_is_rejected_even_if_it_exists() =>
        Assert.Equal(CitationProblem.NotInSources, Check("152", "1", "Pracownikowi przysługuje prawo do corocznego").Problem);

    [Fact]
    public void A_paragraph_that_does_not_exist_is_rejected() =>
        Assert.Equal(CitationProblem.NoSuchParagraph, Check("154", "9", "Wymiar urlopu wynosi").Problem);

    [Fact]
    public void Superscripts_can_be_written_either_way()
    {
        Assert.True(Check("151¹", "1", "przysługuje dodatek w wysokości").Verified);
        Assert.True(Check("36", "1^1", "na zasadach określonych w art. 23^1").Verified);
    }

    [Fact]
    public void The_version_in_force_on_the_date_is_the_one_checked()
    {
        // From 5 November 2026 art. 18³ᵈ grants "zadośćuczynienie"; the old wording said "odszkodowanie w wysokości".
        Assert.True(Check("18^3d", "1", "ma prawo do zadośćuczynienia").Verified);
        Assert.Equal(CitationProblem.NoSuchParagraph,
            CitationCheck.Check(KodeksPracy.Act, new DateOnly(2026, 11, 1), Sources, [new Citation("18^3d", "1", "ma prawo do zadośćuczynienia")]).Single().Problem);
    }

    [Fact]
    public void A_quote_too_short_to_prove_anything_is_not_verified() =>
        Assert.Equal(CitationProblem.QuoteNotFound, Check("154", "1", "20 dni").Problem);
}

/// <summary>The eval set and the parser check each other: every expected unit exists and every evidence quote is in it.</summary>
public class EvalSetTests
{
    private static readonly DateOnly AsOf = new(2026, 11, 23);

    public static TheoryData<string> AnswerableQuestions()
    {
        var data = new TheoryData<string>();
        foreach (var line in File.ReadLines(Path.Combine(KodeksPracy.RepoRoot(), "evals", "questions.jsonl")))
        {
            using var doc = JsonDocument.Parse(line);
            if (doc.RootElement.GetProperty("expected").GetArrayLength() > 0) data.Add(doc.RootElement.GetProperty("id").GetString()!);
        }
        return data;
    }

    private static JsonElement Question(string id) =>
        File.ReadLines(Path.Combine(KodeksPracy.RepoRoot(), "evals", "questions.jsonl"))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Single(e => e.GetProperty("id").GetString() == id);

    [Theory]
    [MemberData(nameof(AnswerableQuestions))]
    public void Expected_units_exist_and_each_evidence_quote_is_in_one_of_them(string id)
    {
        var q = Question(id);
        var units = new List<string>();
        foreach (var expected in q.GetProperty("expected").EnumerateArray())
        {
            var article = KodeksPracy.Act.Find(UnitNumber.Parse(expected.GetProperty("article").GetString()!), AsOf);
            Assert.True(article is not null, $"{id}: art. {expected.GetProperty("article")} not in force on {AsOf}");
            if (expected.TryGetProperty("paragraph", out var p) && p.ValueKind == JsonValueKind.String)
            {
                var paragraph = article.ParagraphsOn(AsOf).FirstOrDefault(x => x.Number == UnitNumber.Parse(p.GetString()!));
                Assert.True(paragraph is not null, $"{id}: art. {article.Key} has no § {p} on {AsOf}");
                units.Add(paragraph.Text);
            }
            else
            {
                units.Add(article.Render(AsOf));
            }
        }
        foreach (var evidence in q.GetProperty("evidence").EnumerateArray().Select(e => e.GetString()!))
        {
            // Evidence was copied from the raw PDF text, so footnote markers may be missing from it; the check
            // is the same one the answers go through.
            Assert.True(units.Any(u => CitationCheck.QuoteIsIn(evidence, u)), $"{id}: evidence not found in the expected units: {evidence}");
        }
    }
}
