using Paragraf.Core.Answers;
using Paragraf.Core.Calculations;

namespace Paragraf.UnitTests;

public sealed class CalculationTests
{
    [Theory]
    [InlineData(2024, 3, 31)]
    [InlineData(2025, 4, 20)]
    [InlineData(2026, 4, 5)]
    [InlineData(2027, 3, 28)]
    public void Easter_falls_on_the_known_dates(int year, int month, int day) =>
        Assert.Equal(new DateOnly(year, month, day), PolishHolidays.EasterSunday(year));

    [Fact]
    public void Christmas_Eve_is_a_holiday_from_2025_only()
    {
        Assert.False(PolishHolidays.IsHoliday(new DateOnly(2024, 12, 24)));
        Assert.True(PolishHolidays.IsHoliday(new DateOnly(2026, 12, 24)));
        Assert.Equal(14, PolishHolidays.For(2026).Count);
    }

    [Theory]
    [InlineData(2026, 1, 160)]  // 1 Jan (Thu) and 6 Jan (Tue) take 16 hours off 176
    [InlineData(2026, 3, 176)]  // no holidays
    [InlineData(2026, 4, 168)]  // Easter Monday 6 Apr
    [InlineData(2026, 8, 160)]  // 15 Aug is a Saturday: it still lowers the norm (art. 130 § 2)
    [InlineData(2026, 11, 160)] // 1 Nov is a Sunday and does not count; 11 Nov (Wed) does
    [InlineData(2026, 12, 160)] // 24, 25 and 26 Dec (Thu, Fri, Sat)
    public void The_working_time_norm_follows_art_130(int year, int month, int hours) =>
        Assert.Equal(hours, LabourCalculations.WorkingTime(year, month).Hours);

    [Fact]
    public void The_norm_agrees_with_counting_working_days_for_every_month_of_two_years()
    {
        foreach (var year in new[] { 2025, 2026 })
        {
            for (var month = 1; month <= 12; month++)
            {
                var days = Enumerable.Range(0, DateTime.DaysInMonth(year, month)).Select(new DateOnly(year, month, 1).AddDays).ToList();
                var workdays = days.Count(d => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !PolishHolidays.IsHoliday(d));
                var saturdayHolidays = days.Count(d => d.DayOfWeek == DayOfWeek.Saturday && PolishHolidays.IsHoliday(d));
                Assert.Equal(8 * (workdays - saturdayHolidays), LabourCalculations.WorkingTime(year, month).Hours);
            }
        }
    }

    [Theory]
    [InlineData("2026-06-01", "2026-11-04", "2 weeks", "2026-11-21")]  // Wednesday: two weeks from Sunday 8 Nov
    [InlineData("2026-06-01", "2026-11-07", "2 weeks", "2026-11-21")]  // Saturday
    [InlineData("2026-06-01", "2026-11-08", "2 weeks", "2026-11-28")]  // Sunday: the next Sunday starts the period
    [InlineData("2026-05-15", "2026-11-15", "1 month", "2026-12-31")]  // exactly 6 months
    [InlineData("2026-05-16", "2026-11-15", "2 weeks", "2026-12-05")]  // a day short of 6 months; given on a Sunday
    [InlineData("2023-11-15", "2026-11-15", "3 months", "2027-02-28")] // exactly 3 years
    [InlineData("2023-11-16", "2026-11-15", "1 month", "2026-12-31")]
    public void Notice_period_and_its_end(string since, string given, string period, string endsOn)
    {
        var notice = LabourCalculations.Notice(DateOnly.Parse(since), DateOnly.Parse(given));
        Assert.Equal(period, notice.Period);
        Assert.Equal(DateOnly.Parse(endsOn), notice.EndsOn);
    }

    [Fact]
    public void Overtime_pay_is_normal_pay_plus_the_allowance_from_the_base_salary()
    {
        // March 2026: 176 hours, so 14 080 / 176 = 80.00 an hour.
        var pay = LabourCalculations.Overtime(14_080m, "PLN", 2026, 3, hoursAt50: 10, hoursAt100: 4);
        Assert.Equal(80.00m, pay.HourlyRate);
        Assert.Equal(1_120.00m, pay.NormalPay);
        Assert.Equal(400.00m, pay.AllowanceAt50);
        Assert.Equal(320.00m, pay.AllowanceAt100);
        Assert.Equal(1_840.00m, pay.Total);
    }

    [Fact]
    public void Overtime_amounts_are_rounded_to_the_grosz()
    {
        // November 2026: 160 hours; 9 999 / 160 = 62.49375.
        var pay = LabourCalculations.Overtime(9_999m, "PLN", 2026, 11, hoursAt50: 3, hoursAt100: 0);
        Assert.Equal(62.49m, pay.HourlyRate);
        Assert.Equal(187.48m, pay.NormalPay);   // 187.48125
        Assert.Equal(93.74m, pay.AllowanceAt50); // 93.740625
        Assert.Equal(281.22m, pay.Total);
    }

    [Fact]
    public void Non_compete_compensation_is_at_least_a_quarter_of_the_pay_for_the_ban()
    {
        var compensation = LabourCalculations.NonCompete(12_800m, "PLN", banMonths: 6);
        Assert.Equal(3_200m, compensation.MinimumPerMonth);
        Assert.Equal(19_200m, compensation.MinimumTotal);
    }

    [Fact]
    public void Deduction_limits_follow_art_87_and_108()
    {
        var limits = LabourCalculations.Deductions(netPay: 6_000m, payAfterDeductions: null);
        Assert.Equal(3_600m, limits.AlimonyEnforcementMax);
        Assert.Equal(3_000m, limits.OtherEnforcementOrAdvancesMax);
        Assert.Equal(600m, limits.MonetaryPenaltiesMax);

        var penalties = LabourCalculations.Deductions(netPay: null, payAfterDeductions: 4_000m);
        Assert.Equal(400m, penalties.MonetaryPenaltiesMax);
        Assert.Null(penalties.AlimonyEnforcementMax);
        Assert.DoesNotContain(penalties.Basis, b => b.Article == "87");
    }

    [Theory]
    [InlineData(11, 0, Education.None, 26)]           // years with every employer count (art. 154¹)
    [InlineData(2, 0, Education.Higher, 26)]          // 2 + 8 for a degree = 10
    [InlineData(1, 11, Education.Higher, 20)]         // 9 years 11 months
    [InlineData(5, 0, Education.SecondaryGeneral, 20)]
    [InlineData(5, 0, Education.PostSecondary, 26)]
    public void Annual_leave_is_20_or_26_days_by_years_with_school_counted(int years, int months, Education education, int days) =>
        Assert.Equal(days, LabourCalculations.Leave(years, months, education).Days);

    [Theory]
    [InlineData(0.5, 26, 13)]
    [InlineData(0.75, 20, 15)]
    [InlineData(0.6, 26, 16)] // 15.6 rounds up (art. 154 § 2)
    public void Part_time_leave_is_pro_rata_rounded_up(double fraction, int fullTime, int days)
    {
        var leave = LabourCalculations.Leave(fullTime == 26 ? 10 : 1, 0, Education.None, (decimal)fraction);
        Assert.Equal(days, leave.Days);
        Assert.Equal(days * 8, leave.Hours);
    }

    [Fact]
    public void Leave_for_part_of_a_year_counts_months_and_rounds_up()
    {
        // Leaving on 15 May: January to May is 5 months, a part month counted whole; 26 × 5/12 = 10.83 → 11.
        var leaving = LabourCalculations.Leave(12, 0, Education.None, workedUntil: new DateOnly(2026, 5, 15));
        Assert.Equal(5, leaving.ProportionalMonths);
        Assert.Equal(11, leaving.ProportionalDays);

        // Starting on 20 September for the rest of the year: 4 months; 20 × 4/12 = 6.67 → 7.
        var joining = LabourCalculations.Leave(3, 0, Education.None, workedFrom: new DateOnly(2026, 9, 20));
        Assert.Equal(4, joining.ProportionalMonths);
        Assert.Equal(7, joining.ProportionalDays);
        Assert.Contains(joining.Basis, b => b.Article == "155^1");
    }

    [Fact]
    public void Every_unit_a_calculation_applies_exists_in_the_code_today()
    {
        var today = new DateOnly(2026, 11, 23);
        var bases = new[]
        {
            LabourCalculations.WorkingTime(2026, 3).Basis,
            LabourCalculations.Notice(new DateOnly(2020, 1, 1), today).Basis,
            LabourCalculations.Overtime(10_000m, "PLN", 2026, 3, 1, 1).Basis,
            LabourCalculations.NonCompete(10_000m, "PLN", 6).Basis,
            LabourCalculations.Deductions(5_000m, 4_000m).Basis,
            LabourCalculations.Leave(5, 0, Education.Higher, 0.5m, workedUntil: today).Basis,
        }.SelectMany(b => b);
        foreach (var unit in bases)
        {
            var article = KodeksPracy.Act.Find(Paragraf.Core.Law.UnitNumber.Parse(unit.Article), today);
            Assert.True(article is not null, $"art. {unit.Article}");
            if (unit.Paragraph is null) continue;
            var paragraph = Paragraf.Core.Law.UnitNumber.Parse(unit.Paragraph);
            Assert.True(article!.ParagraphsOn(today).Any(p => p.Number == paragraph && !p.Repealed), $"art. {unit.Article} § {unit.Paragraph}");
        }
    }

    [Theory]
    [InlineData("Dodatek wynosi 1 840,00 zł.", 1840.00)]
    [InlineData("The total is PLN 1,840.00 gross.", 1840.00)]
    [InlineData("Stawka: 80 zł za godzinę", 80)]
    [InlineData("12 800 PLN miesięcznie", 12800)]
    [InlineData("12.800,50 złotych", 12800.50)]
    [InlineData("62,49 zł", 62.49)]
    [InlineData("€ 3 200", 3200)]
    public void Amounts_are_found_in_either_notation(string text, double value)
    {
        var amount = Assert.Single(AmountCheck.Find(text));
        Assert.Equal((decimal)value, amount.Value);
    }

    [Fact]
    public void Dates_hours_and_articles_are_not_amounts()
    {
        Assert.Empty(AmountCheck.Find("Art. 151¹ § 1: 50% za 10 godzin, do 31.12.2026, 176 godzin w marcu."));
    }

    [Fact]
    public void An_amount_the_tools_did_not_return_is_not_verified()
    {
        var amounts = AmountCheck.Check("Normalne wynagrodzenie 1 120,00 zł, dodatek 400 zł, razem 1 600 zł.", [1120m, 400m, 320m, 1840m]);
        Assert.Equal([true, true, false], amounts.Select(a => a.Verified));
    }
}
