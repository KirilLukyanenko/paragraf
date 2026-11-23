using Paragraf.Core.Law;

namespace Paragraf.UnitTests;

public class UnitNumberTests
{
    [Theory]
    [InlineData("151", 151, null)]
    [InlineData("151^1", 151, "1")]
    [InlineData("151[1]", 151, "1")]
    [InlineData("151(1)", 151, "1")]
    [InlineData("151¹", 151, "1")]
    [InlineData("18^3ca", 18, "3ca")]
    [InlineData("18³ᵈ", 18, "3d")]
    [InlineData(" 67^18 ", 67, "18")]
    public void Parses_every_spelling(string text, int main, string? sup)
    {
        Assert.True(UnitNumber.TryParse(text, out var number));
        Assert.Equal(new UnitNumber(main, sup), number);
    }

    [Theory]
    [InlineData("")]
    [InlineData("art. 5")]
    [InlineData("151^")]
    [InlineData("x1")]
    public void Rejects_what_is_not_a_number(string text) => Assert.False(UnitNumber.TryParse(text, out _));

    [Fact]
    public void Orders_superscripts_by_number_then_letters()
    {
        var order = new[] { "18", "18^1", "18^2", "18^3", "18^3a", "18^3b", "18^3ca", "18^3d", "18^10", "19" }
            .Select(UnitNumber.Parse).ToList();
        Assert.Equal(order, order.OrderBy(n => n).ToList());
    }

    [Fact]
    public void Displays_with_unicode_superscripts() => Assert.Equal("18³ᵈ", UnitNumber.Parse("18^3d").ToDisplay());
}
