namespace Paragraf.Core.Law;

/// <summary>Dates as Polish acts write them: <c>5 listopada 2026</c> (month in the genitive).</summary>
public static class PolishDate
{
    private static readonly string[] Months =
    [
        "stycznia", "lutego", "marca", "kwietnia", "maja", "czerwca",
        "lipca", "sierpnia", "września", "października", "listopada", "grudnia",
    ];

    public static bool TryParse(string text, out DateOnly date)
    {
        date = default;
        var parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return false;
        var month = Array.IndexOf(Months, parts[1].ToLowerInvariant()) + 1;
        if (month == 0 || !int.TryParse(parts[0], out var day) || !int.TryParse(parts[2], out var year)) return false;
        if (day < 1 || day > DateTime.DaysInMonth(year, month)) return false;
        date = new DateOnly(year, month, day);
        return true;
    }
}
