namespace Paragraf.Core.Calculations;

/// <summary>
/// Public holidays in Poland. They are set by the Act of 18 January 1951 on days free from work, not by the Labour
/// Code, so every calculation that uses them says so. Christmas Eve is a holiday from 2025.
/// </summary>
public static class PolishHolidays
{
    public static IReadOnlyList<(DateOnly Date, string Name)> For(int year)
    {
        var easter = EasterSunday(year);
        var days = new List<(DateOnly, string)>
        {
            (new(year, 1, 1), "Nowy Rok"),
            (new(year, 1, 6), "Święto Trzech Króli"),
            (easter, "Wielkanoc"),
            (easter.AddDays(1), "Poniedziałek Wielkanocny"),
            (new(year, 5, 1), "Święto Pracy"),
            (new(year, 5, 3), "Święto Konstytucji 3 Maja"),
            (easter.AddDays(49), "Zielone Świątki"),
            (easter.AddDays(60), "Boże Ciało"),
            (new(year, 8, 15), "Wniebowzięcie Najświętszej Maryi Panny"),
            (new(year, 11, 1), "Wszystkich Świętych"),
            (new(year, 11, 11), "Narodowe Święto Niepodległości"),
            (new(year, 12, 25), "Boże Narodzenie (pierwszy dzień)"),
            (new(year, 12, 26), "Boże Narodzenie (drugi dzień)"),
        };
        if (year >= 2025) days.Add((new(year, 12, 24), "Wigilia Bożego Narodzenia"));
        return days.OrderBy(d => d.Item1).ToList();
    }

    public static bool IsHoliday(DateOnly date) => For(date.Year).Any(h => h.Date == date);

    /// <summary>Easter Sunday in the Gregorian calendar (the anonymous Gregorian algorithm).</summary>
    public static DateOnly EasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }
}
