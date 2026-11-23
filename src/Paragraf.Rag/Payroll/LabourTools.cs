using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Paragraf.Core.Calculations;
using Paragraf.Core.Law;

namespace Paragraf.Rag.Payroll;

/// <summary>One tool call the model made while answering, with what code returned.</summary>
public sealed record ToolCall(string Name, string? CallId, int Iteration, JsonElement Arguments, JsonElement Result, bool Failed, TimeSpan Elapsed)
{
    /// <summary>Every number in the result: the amounts the answer may quote.</summary>
    public IEnumerable<decimal> Numbers => NumbersIn(Result);

    /// <summary>The units of the code the calculation applied ("151^1" and its paragraphs).</summary>
    public IEnumerable<string> BasisArticles =>
        Result.ValueKind == JsonValueKind.Object && Result.TryGetProperty("basis", out var basis) && basis.ValueKind == JsonValueKind.Array
            ? basis.EnumerateArray().Select(b => b.GetProperty("article").GetString()!).Distinct()
            : [];

    private static IEnumerable<decimal> NumbersIn(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => [e.GetDecimal()],
        JsonValueKind.Object => e.EnumerateObject().SelectMany(p => NumbersIn(p.Value)),
        JsonValueKind.Array => e.EnumerateArray().SelectMany(NumbersIn),
        _ => [],
    };
}

/// <summary>
/// The calculations the model may ask for while answering. Code computes every amount and every date; the model
/// picks the calculation and its inputs, then quotes the result. Each result carries the text of the units it
/// applies, as they read on the answer's date, so the answer can cite them.
/// One instance per answer: it records the calls.
/// </summary>
public sealed class LabourTools(LawLibrary library, ICompensationSource? compensation, DateOnly asOf)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly List<ToolCall> _calls = [];

    public IReadOnlyList<ToolCall> Calls
    {
        get { lock (_calls) return _calls.ToList(); }
    }

    public const string Instructions = """
        Calculations:
        - You have tools that calculate by the rules of the code: the working-time norm of a month, the notice period
          and its end, overtime pay, the minimum non-compete compensation, the limits of deductions from pay, and an
          employee's pay from the payroll system (employees have ids like E-1001). Use them whenever the question asks
          for an amount, a number of hours or a date that follows from a rule; do not compute these yourself.
        - Copy every amount and date from the tool results exactly, with its currency (for example 1 840,00 zł).
          A program checks that each amount of money in your answer came from a tool or from the question. Do not
          add, round or convert amounts. If no tool gives an amount the user asks for, state the rule (the share, the
          formula) and say the amount was not calculated.
        - Each result names the units it applied ("basis", with their text): cite them like the sources. Mention the
          assumptions of the result that matter for the user.
        - If a tool returns an error (unknown employee, no pay on that date), say so; do not guess the missing data.
        """;

    public IList<AITool> Functions()
    {
        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(
                ([Description("Year, for example 2026.")] int year, [Description("Month, 1 to 12.")] int month) =>
                    Run("working_time_norm", new { year, month }, () => LabourCalculations.WorkingTime(year, month)),
                "working_time_norm",
                "Hours to work in a calendar month for a full-time employee (art. 130), with the holidays that lower it."),

            AIFunctionFactory.Create(
                async ([Description("The day notice is given, yyyy-MM-dd.")] string notice_given_on,
                       [Description("The first day of employment with this employer, yyyy-MM-dd, if the user gave it.")] string? employed_since = null,
                       [Description("The employee's id in the payroll system, if the question names one and gives no start date.")] string? employee_id = null) =>
                    await RunAsync("notice_period", new { notice_given_on, employed_since, employee_id }, async ct =>
                    {
                        var given = ParseDate(notice_given_on, nameof(notice_given_on));
                        var assumptions = new List<string>();
                        DateOnly since;
                        if (employed_since is not null) since = ParseDate(employed_since, nameof(employed_since));
                        else if (employee_id is not null)
                        {
                            var history = await Source().GetHistoryAsync(employee_id, ct);
                            since = history.Periods.Count > 0 ? history.Periods.Min(p => p.EffectiveFrom)
                                : throw new CompensationUnavailableException("comp-ledger has no records for this employee.");
                            assumptions.Add("The start of employment is taken as the date of the employee's first pay record in the payroll system.");
                        }
                        else throw new ArgumentException("Give employed_since or employee_id.");
                        var notice = LabourCalculations.Notice(since, given);
                        return notice with { Assumptions = [.. assumptions, .. notice.Assumptions] };
                    }),
                "notice_period",
                "The notice period an employer must give (art. 36 § 1) and the day it ends (art. 30 § 2¹)."),

            AIFunctionFactory.Create(
                async ([Description("Year of the month the overtime was worked.")] int year,
                       [Description("Month, 1 to 12.")] int month,
                       [Description("Overtime hours on ordinary days (50% allowance).")] decimal hours_at_50,
                       [Description("Overtime hours at night, on Sundays or holidays that are not working days (100% allowance).")] decimal hours_at_100,
                       [Description("The employee's id in the payroll system, if the question names one.")] string? employee_id = null,
                       [Description("The monthly base salary in PLN, if the user gave it instead of an employee id.")] decimal? monthly_base_salary = null) =>
                    await RunAsync("overtime_pay", new { year, month, hours_at_50, hours_at_100, employee_id, monthly_base_salary }, async ct =>
                    {
                        var lastDay = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
                        var payroll = monthly_base_salary is null ? await PayrollAsync(employee_id, lastDay, includeAllowance: false, ct) : null;
                        var pay = LabourCalculations.Overtime(payroll?.MonthlyPay ?? monthly_base_salary!.Value, payroll?.Currency ?? "PLN",
                            year, month, hours_at_50, hours_at_100);
                        return WithPayroll(pay, payroll);
                    }),
                "overtime_pay",
                "Pay for overtime hours in a month: normal pay for the hours plus the 50% or 100% allowance (art. 151¹)."),

            AIFunctionFactory.Create(
                async ([Description("How many months the non-compete lasts after employment.")] int ban_months,
                       [Description("The employee's id in the payroll system, if the question names one.")] string? employee_id = null,
                       [Description("The last day of employment, yyyy-MM-dd, to read the pay on; today's date of the sources if not given.")] string? last_day_of_employment = null,
                       [Description("The monthly pay in PLN, if the user gave it instead of an employee id.")] decimal? monthly_pay = null) =>
                    await RunAsync("non_compete_compensation", new { ban_months, employee_id, last_day_of_employment, monthly_pay }, async ct =>
                    {
                        var lastDay = last_day_of_employment is null ? asOf : ParseDate(last_day_of_employment, nameof(last_day_of_employment));
                        var payroll = monthly_pay is null ? await PayrollAsync(employee_id, lastDay, includeAllowance: true, ct) : null;
                        var compensation = LabourCalculations.NonCompete(payroll?.MonthlyPay ?? monthly_pay!.Value, payroll?.Currency ?? "PLN", ban_months);
                        return WithPayroll(compensation, payroll);
                    }),
                "non_compete_compensation",
                "The minimum compensation for a non-compete after employment (art. 101² § 3)."),

            AIFunctionFactory.Create(
                ([Description("Monthly pay after social insurance, income tax advance and PPK contributions, if the user gave it.")] decimal? net_pay = null,
                 [Description("Pay left after enforcement and advances have been deducted, if the user gave it (the base for monetary penalties).")] decimal? pay_after_deductions = null) =>
                    Run("deduction_limits", new { net_pay, pay_after_deductions }, () => LabourCalculations.Deductions(net_pay, pay_after_deductions)),
                "deduction_limits",
                "The most that may be deducted from pay: alimony and other enforcement, advances (art. 87 § 3–4) and monetary penalties (art. 108 § 3)."),
        };

        if (compensation is not null)
        {
            tools.Add(AIFunctionFactory.Create(
                async ([Description("The employee's id, for example E-1001.")] string employee_id,
                       [Description("The date the pay must be in effect on, yyyy-MM-dd; the date of the sources if not given.")] string? as_of = null) =>
                    await RunAsync("get_employee_pay", new { employee_id, as_of }, async ct =>
                    {
                        var date = as_of is null ? asOf : ParseDate(as_of, nameof(as_of));
                        var data = await Source().GetAsync(employee_id, date, ct);
                        return new
                        {
                            data.EmployeeId,
                            AsOf = date,
                            Components = data.Components.Select(c => new
                            {
                                c.Component,
                                AnnualAmount = c.Amount,
                                MonthlyAmount = c.Component == "BONUS_TARGET_PCT" ? (decimal?)null : LabourCalculations.Money(c.Amount / 12),
                                c.Currency,
                                c.EffectiveFrom,
                                c.EffectiveTo,
                            }),
                            Assumptions = new[] { "Monthly amounts are annual amounts divided by 12. BONUS_TARGET_PCT is a target percentage, not pay." },
                        };
                    }),
                "get_employee_pay",
                "An employee's pay components in effect on a date, from the payroll system (comp-ledger)."));
        }
        return tools;
    }

    private ICompensationSource Source() =>
        compensation ?? throw new CompensationUnavailableException("The payroll system is not connected; ask the user for the amount.");

    /// <summary>The pay a calculation used, as the payroll system gave it: the amounts are numbers the answer may quote.</summary>
    private sealed record PayrollData(
        string EmployeeId, DateOnly AsOf, decimal AnnualBaseSalary, decimal? AnnualAllowance, string Currency, decimal MonthlyPay, string Note);

    /// <summary>Monthly pay from comp-ledger's annual amounts: the base salary, and the fixed allowance if asked.</summary>
    private async Task<PayrollData> PayrollAsync(string? employeeId, DateOnly date, bool includeAllowance, CancellationToken ct)
    {
        if (employeeId is null) throw new ArgumentException("Give employee_id or the amount.");
        var data = await Source().GetAsync(employeeId, date, ct);
        var salary = data.Find(EmployeeCompensation.BaseSalary)
            ?? throw new CompensationUnavailableException($"No base salary for this employee on {date:yyyy-MM-dd}.");
        var allowance = includeAllowance ? data.Find(EmployeeCompensation.Allowance) : null;
        if (allowance is not null && allowance.Currency != salary.Currency)
        {
            throw new CompensationUnavailableException("The base salary and the allowance are in different currencies.");
        }
        var monthly = LabourCalculations.Money((salary.Amount + (allowance?.Amount ?? 0)) / 12);
        return new PayrollData(employeeId, date, salary.Amount, allowance?.Amount, salary.Currency ?? "PLN", monthly,
            "Read from the payroll system (comp-ledger) as in effect on as_of. Amounts there are annual; the monthly pay is "
            + (allowance is null ? "the annual base salary" : "the annual base salary plus the annual allowance") + " divided by 12.");
    }

    /// <summary>The calculation, with the payroll data it used under "payroll".</summary>
    private static object WithPayroll(object calculation, PayrollData? payroll)
    {
        if (payroll is null) return calculation;
        var node = JsonSerializer.SerializeToNode(calculation, calculation.GetType(), Json)!.AsObject();
        node["payroll"] = JsonSerializer.SerializeToNode(payroll, Json);
        return node;
    }

    private JsonElement Run(string name, object arguments, Func<object> calculate) =>
        RunAsync(name, arguments, _ => Task.FromResult(calculate())).GetAwaiter().GetResult();

    private async Task<JsonElement> RunAsync(string name, object arguments, Func<CancellationToken, Task<object>> calculate)
    {
        var watch = Stopwatch.StartNew();
        var context = FunctionInvokingChatClient.CurrentContext;
        JsonElement result;
        var failed = false;
        try
        {
            var value = await calculate(CancellationToken.None);
            result = WithBasisText(JsonSerializer.SerializeToElement(value, value.GetType(), Json));
        }
        catch (Exception ex) when (ex is ArgumentException or CompensationUnavailableException or FormatException)
        {
            failed = true;
            result = JsonSerializer.SerializeToElement(new { Error = ex.Message }, Json);
        }
        lock (_calls)
        {
            _calls.Add(new ToolCall(name, context?.CallContent.CallId, context?.Iteration ?? 0, JsonSerializer.SerializeToElement(arguments, Json), result, failed, watch.Elapsed));
        }
        return result;
    }

    /// <summary>Adds the text of each unit in "basis" as it reads on the answer's date.</summary>
    private JsonElement WithBasisText(JsonElement result)
    {
        if (!result.TryGetProperty("basis", out var basis)) return result;
        var withText = basis.EnumerateArray().Select(b =>
        {
            var article = b.GetProperty("article").GetString()!;
            var paragraph = b.TryGetProperty("paragraph", out var p) ? p.GetString() : null;
            return new { Article = article, Paragraph = paragraph, Text = UnitText(article, paragraph) };
        }).ToList();
        var node = System.Text.Json.Nodes.JsonNode.Parse(result.GetRawText())!.AsObject();
        node["basis"] = JsonSerializer.SerializeToNode(withText, Json);
        return JsonSerializer.SerializeToElement(node, Json);
    }

    private string? UnitText(string article, string? paragraph)
    {
        if (!UnitNumber.TryParse(article, out var number) || library.Act.Find(number, asOf) is not { } found) return null;
        if (paragraph is null) return found.Render(asOf);
        return UnitNumber.TryParse(paragraph, out var p) && found.ParagraphsOn(asOf).FirstOrDefault(x => x.Number == p) is { } par
            ? $"Art. {found.Number.ToDisplay()} § {par.Number.ToDisplay()}. {par.Text}"
            : null;
    }

    private static DateOnly ParseDate(string text, string name) =>
        DateOnly.TryParseExact(text, "yyyy-MM-dd", out var date) ? date : throw new FormatException($"{name} must be a date as yyyy-MM-dd.");
}
