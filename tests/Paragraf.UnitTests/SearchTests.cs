using Paragraf.Core.Law;
using Paragraf.Core.Search;

namespace Paragraf.UnitTests;

public class SearchTests
{
    [Theory]
    [InlineData("Co mówi art. 151¹ § 2?", "151^1", "2")]
    [InlineData("artykuł 36 § 1 kodeksu", "36", "1")]
    [InlineData("What does article 151(1) say?", "151^1", null)]
    [InlineData("art 154", "154", null)]
    [InlineData("Що каже ст. 154?", "154", null)]
    [InlineData("відповідно до статті 36", "36", null)]
    [InlineData("art. 18^3d", "18^3d", null)]
    public void Finds_named_articles(string question, string article, string? paragraph)
    {
        var found = Assert.Single(ArticleReference.Find(question));
        Assert.Equal(UnitNumber.Parse(article), found.Article);
        Assert.Equal(paragraph is null ? null : UnitNumber.Parse(paragraph), found.Paragraph);
    }

    [Theory]
    [InlineData("Ile dni urlopu mi przysługuje?")]
    [InlineData("I started in 2020, what is my notice period?")]
    [InlineData("partner w firmie")]
    public void Finds_nothing_when_no_article_is_named(string question) => Assert.Empty(ArticleReference.Find(question));

    [Fact]
    public void Lexical_terms_match_across_inflection_and_diacritics()
    {
        Assert.Equal(LexicalTerms.Of("wynagrodzenia"), LexicalTerms.Of("wynagrodzeniu"));
        Assert.Equal(["wynagr", "urlopu", "wypocz"], LexicalTerms.Of("Wynagrodzenie za urlop wypoczynkowy").Take(1).Concat(LexicalTerms.Of("urlopu wypoczynkowego")));
        Assert.Equal(["zaswia", "lekars"], LexicalTerms.Of("Zaświadczenie lekarskie"));
        Assert.Equal(["mlodoc"], LexicalTerms.Of("młodocianych"));
    }

    [Fact]
    public void Rank_fusion_rewards_agreement_between_lists()
    {
        var fused = RankFusion.Fuse([
            ("vector", ["a", "b", "c"], 1.0),
            ("lexical", ["c", "d", "a"], 1.0),
        ]);
        Assert.Equal("a", fused[0].Id);       // 1st and 3rd
        Assert.Equal("c", fused[1].Id);       // 3rd and 1st, ties broken by id
        Assert.Equal(new Dictionary<string, int> { ["vector"] = 1, ["lexical"] = 3 }, fused[0].Ranks);
        Assert.Equal(4, fused.Count);
    }

    [Fact]
    public void Chunks_cover_every_paragraph_in_force_and_skip_repealed_ones()
    {
        var chunks = Chunker.Chunk(KodeksPracy.Act);
        Assert.Contains(chunks, c => c.Id == "151^1§1");
        Assert.Contains(chunks, c => c.Id == "1");
        Assert.DoesNotContain(chunks, c => c.Id.StartsWith("36§2", StringComparison.Ordinal)); // repealed
        Assert.Equal(chunks.Count, chunks.Select(c => c.Id).Distinct().Count());
        Assert.Contains(chunks, c => c.Id == "18^3d@2026-11-05§1" || c.Id == "18^3d§1@2026-11-05");
        Assert.Contains(chunks, c => c.Id == "18^3d@until-2026-11-05");
        var overtime = chunks.Single(c => c.Id == "151^1§1");
        Assert.StartsWith("Kodeks pracy › DZIAŁ SZÓSTY – Czas pracy › Rozdział V – Praca w godzinach nadliczbowych\nArt. 151¹. § 1. Za pracę", overtime.Content);
        Assert.Equal("art. 151¹ § 1", overtime.Citation);
    }
}
