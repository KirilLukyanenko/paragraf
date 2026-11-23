using Paragraf.Core.Law;

namespace Paragraf.UnitTests;

public class LawParserTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);
    private static readonly DateOnly BeforeAmendment = new(2026, 11, 4);
    private static Act Act => KodeksPracy.Act;

    private static Article InForce(string number, DateOnly? date = null) =>
        Act.Find(UnitNumber.Parse(number), date ?? Today) ?? throw new Xunit.Sdk.XunitException($"art. {number} not found");

    [Fact]
    public void Footnotes_are_numbered_without_gaps()
    {
        var split = DziennikUstawText.Read(KodeksPracy.Text, "Załącznik do obwieszczenia");
        Assert.Equal(Enumerable.Range(1, split.Footnotes.Count), split.Footnotes.Keys.Order());
        Assert.True(split.Footnotes.Count >= 60, $"only {split.Footnotes.Count} footnotes found");
    }

    [Fact]
    public void Articles_run_from_1_to_305_in_text_order()
    {
        Assert.Equal("1", Act.Articles[0].Key);
        Assert.Equal("305", Act.Articles[^1].Key);
        for (var i = 1; i < Act.Articles.Count; i++)
        {
            Assert.True(Act.Articles[i].Number.CompareTo(Act.Articles[i - 1].Number) >= 0,
                $"art. {Act.Articles[i].Key} comes after art. {Act.Articles[i - 1].Key}");
        }
    }

    [Fact]
    public void No_page_header_or_footnote_text_leaks_into_articles()
    {
        foreach (var article in Act.Articles)
        {
            foreach (var date in new[] { BeforeAmendment, Today })
            {
                var text = article.Render(date);
                Assert.DoesNotContain("Dziennik Ustaw", text);
                Assert.DoesNotContain("W brzmieniu ustalonym przez", text);
                Assert.DoesNotContain("Dodany przez art.", text);
                Assert.DoesNotContain("W tym brzmieniu obowiązuje", text);
            }
        }
    }

    [Fact]
    public void Each_article_has_at_most_one_version_in_force_on_a_date()
    {
        foreach (var date in new[] { BeforeAmendment, Today })
        {
            var duplicates = Act.Articles.Where(a => a.Validity.IsInForceOn(date))
                .GroupBy(a => a.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.Empty(duplicates);
            foreach (var article in Act.Articles.Where(a => a.Validity.IsInForceOn(date)))
            {
                var paragraphs = article.ParagraphsOn(date).GroupBy(p => p.Number).Where(g => g.Count() > 1).ToList();
                Assert.True(paragraphs.Count == 0, $"art. {article.Key} has two versions of § {paragraphs.FirstOrDefault()?.Key} on {date}");
            }
        }
    }

    [Fact]
    public void Overtime_supplement_article_has_its_four_paragraphs()
    {
        var article = InForce("151^1");
        Assert.Equal(["1", "2", "3", "4"], article.Paragraphs.Select(p => p.Number.ToString()));
        Assert.Contains("100 % wynagrodzenia", article.Paragraphs[0].Text);
        Assert.Contains("\n2) 50 % wynagrodzenia", article.Paragraphs[0].Text);
        Assert.Contains("Rozdział V – Praca w godzinach nadliczbowych", article.Headings);
    }

    [Fact]
    public void Words_split_across_lines_are_joined()
    {
        // "obowią-" / "zującym" in the PDF.
        Assert.Contains("zgodnie z obowiązującym go rozkładem czasu pracy", InForce("151^1").Paragraphs[0].Text);
    }

    [Fact]
    public void Paragraph_with_superscript_number_is_kept()
    {
        var article = InForce("36");
        Assert.Contains(article.Paragraphs, p => p.Number == new UnitNumber(1, "1"));
        Assert.True(article.Paragraphs.Single(p => p.Number.ToString() == "2").Repealed);
    }

    [Fact]
    public void List_item_with_a_glued_footnote_marker_starts_a_new_line()
    {
        var text = InForce("22^1").Paragraphs[0].Text;
        Assert.Contains("\n6) przebieg dotychczasowego zatrudnienia", text);
    }

    [Fact]
    public void Amended_article_has_the_old_version_before_and_the_new_one_from_5_November_2026()
    {
        var before = InForce("18^3d", BeforeAmendment);
        var after = InForce("18^3d", Today);
        Assert.Equal(new Validity(null, new DateOnly(2026, 11, 5)), before.Validity);
        Assert.Equal(new Validity(new DateOnly(2026, 11, 5), null), after.Validity);
        Assert.Contains("prawo do odszkodowania w wysokości", before.Lead);
        Assert.Contains("prawo do zadośćuczynienia", after.Paragraphs[0].Text);
    }

    [Fact]
    public void Amended_paragraph_switches_versions_inside_one_article()
    {
        var article = InForce("18^3a");
        var before = article.ParagraphsOn(BeforeAmendment).Select(p => p.Number.ToString()).ToList();
        var after = article.ParagraphsOn(Today).Select(p => p.Number.ToString()).ToList();
        Assert.DoesNotContain("4^1", before);
        Assert.Contains("4^1", after);
        Assert.Single(after, n => n == "2");
    }

    [Fact]
    public void Repealed_article_is_marked()
    {
        var article = InForce("4");
        Assert.True(article.Repealed);
        Assert.Equal("Art. 4. (uchylony)", article.Render(Today));
    }

    [Fact]
    public void Leave_entitlement_reads_as_in_the_code()
    {
        Assert.Equal(
            "Art. 154.\n§ 1. Wymiar urlopu wynosi:\n1) 20 dni – jeżeli pracownik jest zatrudniony krócej niż 10 lat;\n2) 26 dni – jeżeli pracownik jest zatrudniony co najmniej 10 lat.",
            InForce("154").Render(Today).Split("\n§ 2.")[0]);
    }
}

public class LawParserEdgeCaseTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);
    private static Article InForce(string number) => KodeksPracy.Act.Find(UnitNumber.Parse(number), Today)!;

    [Fact]
    public void A_footnote_continued_on_the_next_page_stays_out_of_the_article()
    {
        var text = InForce("3^1").Render(Today);
        Assert.DoesNotContain("dyrektywy", text);
        Assert.EndsWith("czynności, o których mowa w tym przepisie.", text);
    }

    [Fact]
    public void Hours_printed_with_superscript_minutes_read_as_times()
    {
        Assert.Contains("między godzinami 21:00 a 7:00", InForce("151^7").Render(Today));
        Assert.Contains("pomiędzy godzinami 22:00 a 6:00", InForce("203").Render(Today));
    }

    [Fact]
    public void A_removed_range_of_articles_does_not_attach_to_the_previous_article()
    {
        Assert.DoesNotContain("266", InForce("265").Render(Today));
        Assert.Null(KodeksPracy.Act.Find(UnitNumber.Parse("270"), Today));
    }

    [Fact]
    public void Lapsed_units_are_marked_like_repealed_ones() => Assert.True(InForce("103").Repealed);

    [Fact]
    public void References_to_superscript_articles_read_with_superscripts() =>
        Assert.Contains("na zasadach określonych w art. 23¹", InForce("36").Render(Today));

    [Fact]
    public void Every_article_number_in_the_text_is_parsed()
    {
        var inText = System.Text.RegularExpressions.Regex.Matches(KodeksPracy.Text, @"^\s*Art\. (\d+)(?:\[([0-9a-z]+)\])?\.", System.Text.RegularExpressions.RegexOptions.Multiline)
            .Select(m => new UnitNumber(int.Parse(m.Groups[1].Value), m.Groups[2].Success ? m.Groups[2].Value : null).ToString())
            .ToHashSet();
        var parsed = KodeksPracy.Act.Articles.Select(a => a.Key).ToHashSet();
        // Article numbers at the start of a line before the act begins (in the announcement) don't count.
        var missing = inText.Except(parsed).Where(k => UnitNumber.Parse(k).Main > 2).ToList();
        Assert.Empty(missing);
    }
}
