namespace Paragraf.Api;

/// <summary>
/// A daily ceiling on model spend for the public demo: once the day's answers have cost
/// <c>Paragraf:DailyBudgetUsd</c>, the API answers 503 until midnight UTC. One instance, in memory:
/// enough for a demo on one machine, and the rate limiter in front keeps one visitor from using it up.
/// </summary>
public sealed class SpendGuard(decimal dailyBudgetUsd, TimeProvider clock)
{
    private readonly Lock _lock = new();
    private DateOnly _day = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
    private decimal _spent;

    public decimal DailyBudgetUsd => dailyBudgetUsd;

    public bool CanSpend()
    {
        lock (_lock)
        {
            Roll();
            return _spent < dailyBudgetUsd;
        }
    }

    public void Record(decimal usd)
    {
        lock (_lock)
        {
            Roll();
            _spent += usd;
        }
    }

    public decimal SpentToday
    {
        get { lock (_lock) { Roll(); return _spent; } }
    }

    private void Roll()
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        if (today == _day) return;
        _day = today;
        _spent = 0;
    }
}
