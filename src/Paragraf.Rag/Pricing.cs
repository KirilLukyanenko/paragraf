using Microsoft.Extensions.AI;

namespace Paragraf.Rag;

/// <summary>API list prices in USD per million tokens, to put a cost on every answer and every eval run.</summary>
public static class Pricing
{
    private sealed record Price(decimal Input, decimal Output, decimal CacheRead);

    private static readonly Dictionary<string, Price> Models = new(StringComparer.Ordinal)
    {
        ["claude-opus-5-5"] = new(4.00m, 20.00m, 0.20m),
        ["claude-sonnet-5-5"] = new(2.00m, 10.00m, 0.10m),
        ["claude-haiku-5-5"] = new(0.10m, 0.50m, 0.01m),
        ["text-embedding-3-small"] = new(0.02m, 0m, 0m),
    };

    public static decimal Cost(string model, UsageDetails? usage)
    {
        if (usage is null || !Models.TryGetValue(model, out var price)) return 0m;
        var cached = usage.CachedInputTokenCount ?? 0;
        var input = (usage.InputTokenCount ?? 0) - cached;
        var output = usage.OutputTokenCount ?? 0;
        return (input * price.Input + cached * price.CacheRead + output * price.Output) / 1_000_000m;
    }

    public static bool Knows(string model) => Models.ContainsKey(model);
}
