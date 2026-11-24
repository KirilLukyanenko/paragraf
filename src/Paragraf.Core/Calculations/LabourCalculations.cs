namespace Paragraf.Core.Calculations;

/// <summary>The unit of the code a calculation applies. Article keys use a caret for superscripts: "151^1".</summary>
public sealed record LegalBasis(string Article, string? Paragraph);

/// <summary>Hours to work in a calendar month (art. 130 § 1–2).</summary>
public sealed record WorkingTimeNorm(
    int Year,
    int Month,
    int FullWeeks,
    int WeekdaysAfterFullWeeks,
    IReadOnlyList<DateOnly> HolidaysNotOnSunday,
    int Hours,
    IReadOnlyList<LegalBasis> Basis,
    IReadOnlyList<string> Assumptions);

public sealed record NoticePeriod(
    DateOnly EmployedSince,
    DateOnly NoticeGivenOn,
    int TenureYears,
    int TenureMonths,
    string Period,
    DateOnly EndsOn,
    IReadOnlyList<LegalBasis> Basis,
    IReadOnlyList<string> Assumptions);

public sealed record OvertimePay(
    int Year,
    int Month,
    decimal MonthlyBaseSalary,
    string Currency,
    int NormHours,
    decimal HourlyRate,
    decimal HoursAt50,
    decimal HoursAt100,
    decimal NormalPay,
    decimal AllowanceAt50,
    decimal AllowanceAt100,
    decimal Total,
    IReadOnlyList<LegalBasis> Basis,
    IReadOnlyList<string> Assumptions);

public sealed record NonCompeteCompensation(
    decimal MonthlyPay,
    string Currency,
    int BanMonths,
    decimal MinimumPerMonth,
    decimal MinimumTotal,
    IReadOnlyList<LegalBasis> Basis,
    IReadOnlyList<string> Assumptions);

/// <summary>
/// The most that may be deducted from pay (art. 87 § 3–4, art. 108 § 3). Net pay is pay after social insurance, the
/// income tax advance and PPK contributions (art. 87 § 1). A limit is null when its input was not given.
/// </summary>
public sealed record DeductionLimits(
    decimal? NetPay,
    decimal? AlimonyEnforcementMax,
    decimal? OtherEnforcementOrAdvancesMax,
    decimal? AllEnforcementAndAdvancesMax,
    decimal? PayAfterDeductions,
    decimal? MonetaryPenaltiesMax,
    IReadOnlyList<LegalBasis> Basis,
    IReadOnlyList<string> Assumptions);

/// <summary>The school completed, which art. 155 § 1 counts as years of work for leave.</summary>
public enum Education
{
    None,
    BasicVocational,
    SecondaryVocational,
    SecondaryVocationalAfterBasic,
    SecondaryGeneral,
    PostSecondary,
    Higher,
}

/// <summary>Annual leave in days (art. 154, 154¹, 155, 155¹–155³).</summary>
public sealed record AnnualLeave(
    int WorkYears,
    int WorkMonths,
    string Education,
    int EducationYears,
    decimal YearsForLeave,
    int FullTimeDays,
    decimal TimeFraction,
    int Days,
    int Hours,
    DateOnly? ProportionalFrom,
    DateOnly? ProportionalUntil,
    int? ProportionalMonths,
    int? ProportionalDays,
    IReadOnlyList<LegalBasis> Basis,
    IReadOnlyList<string> Assumptions);

/// <summary>
/// Calculations the code sets out as rules: done by code, never by the model. Each result names the units it
/// applies and every assumption that does not come from the code itself.
/// </summary>
public static class LabourCalculations
{
    public const string HolidaysAssumption =
        "Public holidays are taken from the Act on days free from work (outside the Labour Code), Christmas Eve included from 2025.";

    /// <summary>
    /// Art. 130 § 1: 40 hours for each full week of the period, plus 8 hours for each Monday to Friday left after
    /// the full weeks; § 2: minus 8 hours for each holiday in the period that is not a Sunday.
    /// </summary>
    public static WorkingTimeNorm WorkingTime(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var days = DateTime.DaysInMonth(year, month);
        var fullWeeks = days / 7;
        var remaining = Enumerable.Range(fullWeeks * 7, days - fullWeeks * 7)
            .Select(first.AddDays)
            .Count(d => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday));
        var holidays = PolishHolidays.For(year)
            .Select(h => h.Date)
            .Where(d => d.Month == month && d.DayOfWeek != DayOfWeek.Sunday)
            .ToList();
        var hours = 40 * fullWeeks + 8 * remaining - 8 * holidays.Count;
        return new WorkingTimeNorm(year, month, fullWeeks, remaining, holidays, hours,
            [new("130", "1"), new("130", "2")],
            [
                "The settlement period is the calendar month.",
                "Full-time work: 8 hours a day and on average 40 hours in an average five-day working week (art. 129 § 1).",
                HolidaysAssumption,
            ]);
    }

    /// <summary>
    /// Art. 36 § 1: 2 weeks under 6 months of employment with this employer, 1 month from 6 months, 3 months from
    /// 3 years. Art. 30 § 2¹: a period of weeks ends on a Saturday, a period of months on the last day of a month.
    /// </summary>
    public static NoticePeriod Notice(DateOnly employedSince, DateOnly noticeGivenOn)
    {
        if (noticeGivenOn < employedSince)
        {
            throw new ArgumentException("The notice date is before the start of employment.", nameof(noticeGivenOn));
        }
        var (years, months) = Tenure(employedSince, noticeGivenOn);
        string period;
        DateOnly endsOn;
        if (years >= 3 || months + 12 * years >= 6)
        {
            var length = years >= 3 ? 3 : 1;
            period = length == 3 ? "3 months" : "1 month";
            var month = new DateOnly(noticeGivenOn.Year, noticeGivenOn.Month, 1).AddMonths(length);
            endsOn = new DateOnly(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        }
        else
        {
            period = "2 weeks";
            endsOn = noticeGivenOn.AddDays(14);
            while (endsOn.DayOfWeek != DayOfWeek.Saturday) endsOn = endsOn.AddDays(1);
        }
        return new NoticePeriod(employedSince, noticeGivenOn, years, months, period, endsOn,
            [new("36", "1"), new("30", "2^1")],
            [
                "A contract for an indefinite or a fixed term; employment with this employer only (§ 1¹ also counts a previous employer in a transfer under art. 23¹).",
                "Tenure is counted on the day notice is given.",
                "Weeks: the period starts on the Sunday after notice is given and ends on a Saturday. Months: it starts on the first day of the next month and ends on the last day of a month.",
            ]);
    }

    /// <summary>
    /// Art. 151¹ § 1: for each overtime hour the normal pay plus an allowance of 50% (ordinary days) or 100% (at
    /// night, on Sundays and holidays that are not working days); § 3: the allowance is a share of the pay from the
    /// employee's personal grade, here the monthly base salary.
    /// </summary>
    public static OvertimePay Overtime(decimal monthlyBaseSalary, string currency, int year, int month, decimal hoursAt50, decimal hoursAt100)
    {
        if (monthlyBaseSalary <= 0) throw new ArgumentOutOfRangeException(nameof(monthlyBaseSalary), "The base salary must be positive.");
        if (hoursAt50 < 0 || hoursAt100 < 0) throw new ArgumentOutOfRangeException(nameof(hoursAt50), "Hours cannot be negative.");
        var norm = WorkingTime(year, month);
        var hourly = monthlyBaseSalary / norm.Hours;
        var normal = Money(hourly * (hoursAt50 + hoursAt100));
        var at50 = Money(hourly * 0.5m * hoursAt50);
        var at100 = Money(hourly * hoursAt100);
        return new OvertimePay(year, month, monthlyBaseSalary, currency, norm.Hours, Money(hourly), hoursAt50, hoursAt100,
            normal, at50, at100, normal + at50 + at100,
            [new("151^1", "1"), new("151^1", "3"), new("130", "1"), new("130", "2")],
            [
                "The hourly rate is the monthly base salary divided by the hours to work in that month (art. 130); the method comes from the regulation on pay, not from the code.",
                "The base for the allowance is the base salary (personal grade, art. 151¹ § 3); other components such as allowances are not included.",
                "Overtime that is compensated with time off (art. 151² § 3) gets no allowance.",
                "Amounts are rounded to the grosz at each component; the total is their sum.",
                HolidaysAssumption,
            ]);
    }

    /// <summary>
    /// Art. 101² § 3: compensation for a non-compete after employment of at least 25% of the pay received before the
    /// employment ended, for the period of the ban; it may be paid in monthly instalments.
    /// </summary>
    public static NonCompeteCompensation NonCompete(decimal monthlyPay, string currency, int banMonths)
    {
        if (monthlyPay <= 0) throw new ArgumentOutOfRangeException(nameof(monthlyPay), "The monthly pay must be positive.");
        if (banMonths is < 1 or > 120) throw new ArgumentOutOfRangeException(nameof(banMonths), "The ban lasts 1 to 120 months.");
        var perMonth = Money(monthlyPay * 0.25m);
        return new NonCompeteCompensation(monthlyPay, currency, banMonths, perMonth, perMonth * banMonths,
            [new("101^2", "3"), new("101^2", "1")],
            [
                "The pay is the fixed monthly pay before employment ended; variable pay is not included. The code does not define the base further.",
                "This is the minimum: the agreement may set more.",
            ]);
    }

    /// <summary>
    /// Art. 87 § 3: alimony enforcement up to three fifths of net pay; other enforcement or advances up to half; § 4:
    /// together, half and three fifths. Art. 108 § 3: monetary penalties together up to a tenth of the pay left after
    /// the deductions of art. 87 § 1 pkt 1–3.
    /// </summary>
    public static DeductionLimits Deductions(decimal? netPay, decimal? payAfterDeductions)
    {
        if (netPay is null && payAfterDeductions is null) throw new ArgumentException("Give the net pay or the pay left after deductions.");
        if (netPay <= 0 || payAfterDeductions < 0) throw new ArgumentOutOfRangeException(nameof(netPay), "Pay must be positive.");
        var penaltiesBase = payAfterDeductions ?? netPay;
        var basis = new List<LegalBasis>();
        if (netPay is not null) basis.AddRange([new("87", "3"), new("87", "4"), new("87", "1")]);
        basis.Add(new("108", "3"));
        return new DeductionLimits(
            netPay,
            netPay is { } a ? Money(a * 3 / 5) : null,
            netPay is { } b ? Money(b / 2) : null,
            netPay is { } c ? Money(c * 3 / 5) : null,
            payAfterDeductions,
            penaltiesBase is { } p ? Money(p / 10) : null,
            basis,
            [
                "Net pay is pay after social insurance contributions, the income tax advance and PPK contributions (art. 87 § 1).",
                "The amounts free from deductions (art. 87¹) are not applied: they follow from the minimum wage, which is set outside the code. They do not cover alimony enforcement.",
                payAfterDeductions is null
                    ? "Monetary penalties are limited to a tenth of the pay left after the deductions of art. 87 § 1 pkt 1–3; with no such deductions, of the net pay."
                    : "Monetary penalties are limited to a tenth of the pay left after the deductions of art. 87 § 1 pkt 1–3, as given.",
                "A single penalty may not exceed one day's pay (art. 108 § 3).",
            ]);
    }

    /// <summary>Years of school counted as work by art. 155 § 1 (not added up: the highest school completed counts).</summary>
    public static int EducationYears(Education education) => education switch
    {
        Education.BasicVocational => 3,
        Education.SecondaryVocational => 5,
        Education.SecondaryVocationalAfterBasic => 5,
        Education.SecondaryGeneral => 4,
        Education.PostSecondary => 6,
        Education.Higher => 8,
        _ => 0,
    };

    /// <summary>
    /// Art. 154 § 1: 20 days under 10 years, 26 from 10 years; art. 154¹: all previous employment counts; art. 155:
    /// the school completed counts as years of work; art. 154 § 2: part-time pro rata, a part day rounded up.
    /// With a period of work in the year (art. 155¹, 155²ᵃ, 155³): 1/12 for each calendar month, a part month and a
    /// part day rounded up.
    /// </summary>
    public static AnnualLeave Leave(int workYears, int workMonths, Education education, decimal timeFraction = 1m,
        DateOnly? workedFrom = null, DateOnly? workedUntil = null)
    {
        if (workYears < 0 || workMonths is < 0 or > 11) throw new ArgumentOutOfRangeException(nameof(workMonths), "Work is years (0 or more) and months (0 to 11).");
        if (timeFraction is <= 0 or > 1) throw new ArgumentOutOfRangeException(nameof(timeFraction), "The fraction of full time is above 0 and at most 1.");
        var school = EducationYears(education);
        var years = workYears + workMonths / 12m + school;
        var fullTime = years >= 10 ? 26 : 20;
        var days = (int)Math.Ceiling(fullTime * timeFraction);

        int? months = null, proportional = null;
        if (workedFrom is not null || workedUntil is not null)
        {
            var from = workedFrom ?? new DateOnly((workedUntil ?? workedFrom)!.Value.Year, 1, 1);
            var until = workedUntil ?? new DateOnly(from.Year, 12, 31);
            if (until < from || until.Year != from.Year) throw new ArgumentException("The period of work must lie within one calendar year.");
            months = until.Month - from.Month + 1;
            proportional = (int)Math.Ceiling(days * months.Value / 12m);
        }

        var basis = new List<LegalBasis> { new("154", "1"), new("154^1", "1") };
        if (school > 0) basis.Add(new("155", "1"));
        if (timeFraction < 1) basis.Add(new("154", "2"));
        if (months is not null) basis.AddRange([new("155^1", "1"), new("155^2a", "1"), new("155^2a", "2"), new("155^3", "1")]);
        var assumptions = new List<string>
        {
            "Years of work include every previous employer, whatever the breaks (art. 154¹).",
            "Years of school and of work are added as given; if the school was attended while working, only the more favourable of the two counts (art. 155 § 2), which the calculation does not check.",
            "A year of work is the employee's right to leave in the following calendar years; in the very first job, leave is earned month by month (art. 153 § 1), which this does not calculate.",
        };
        if (months is not null)
        {
            assumptions.Add("Proportional leave: each calendar month of work in the year is 1/12 of the leave; a part month and a part day are rounded up.");
            assumptions.Add("Leave already used before the employment ended is not taken into account (art. 155¹ § 1 pkt 1, § 2).");
        }
        return new AnnualLeave(workYears, workMonths, education.ToString(), school, years, fullTime, timeFraction, days,
            days * 8, workedFrom, workedUntil, months, proportional, basis, assumptions);
    }

    /// <summary>Full years and months from the start date to the date (the start day counts as the first day).</summary>
    public static (int Years, int Months) Tenure(DateOnly from, DateOnly to)
    {
        var months = (to.Year - from.Year) * 12 + to.Month - from.Month;
        if (from.AddMonths(months) > to) months--;
        return (months / 12, months % 12);
    }

    public static decimal Money(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
