using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;

namespace UnitTests.Extensions;
public class StringExtensionsTests
{
    [Fact]
    public void CleanUp_ReturnsString()
    {
        var template = "Iron Man";

        var result = template.CleanUp();

        Assert.IsType<string>(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void CleanUp_StringIsUpperCase()
    {
        var template = "Captain America";
        var expected = template.ToUpper();

        var result = template.CleanUp();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("glow-eyes", "glow eyes")]
    [InlineData("mech'tennas", "mech tennas")]
    [InlineData("pop'tennas", "pop tennas")]
    [InlineData("Somnambulist's Totem", "Somnambulist s Totem")]
    [InlineData("509 (Shadow) Slime Lockbox", "509 Shadow Slime Lockbox")]
    public void CleanUp_FiltersImportantSymbols(string template, string expected)
    {
        var result = template.CleanUp();

        Assert.Equal(expected, result, ignoreCase: true);
    }

    [Theory]
    [InlineData("ctr very high asi very high", "asi very high ctr very high")]
    [InlineData("ctr very high asi high", "asi high ctr very high")]
    [InlineData("lite gm", "asi high ctr very high")]
    [InlineData("gm lite", "asi high ctr very high")]
    [InlineData(" gm", "asi very high ctr very high")]
    [InlineData("gm ", "asi very high ctr very high")]
    public void CleanUp_ForcesWeaponUvOrder(string template, string expected)
    {
        var result = template.CleanUp();

        Assert.Equal(expected, result, ignoreCase: true);
    }

    [Theory]
    [InlineData("Brandish")]
    [InlineData("Overcharged Mixmaster")]
    [InlineData("Blast Bomb")]
    [InlineData("Swiftstrike Buckler")]
    [InlineData("Black Kat Cowl")]
    public void ConvertToPunchOption_ConvertsExpectedItems(string template)
    {
        var result = template.ConvertToPunchOption();

        Assert.IsType<PunchOption>(result);
    }

    [Fact]
    public void ConvertToPunchOption_ThrowsExceptionWhenWrongItem()
    {
        var template = "The God of Thunder";

        Assert.Throws<InvalidCastException>(() => template.ConvertToPunchOption());
    }

    [Theory]
    [InlineData("Iron Man", "Iron Man")]
    [InlineData("   Captain   America   ", "Captain America")]
    [InlineData("Black\tWidow", "Black Widow")]
    [InlineData("Bruce\r\nBanner", "Bruce Banner")]
    [InlineData("The God of\u00A0Thunder", "The God of Thunder")]
    [InlineData("Hawkeye", "Hawkeye")]
    public void RemoveExtraWhiteSpace_RemovesExpectedWhiteSpace(string input, string expected)
    {
        var result = input.RemoveExtraWhiteSpace();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToTitleCase_ReturnsStringInTitleCase()
    {
        var template = "avengers age of ultron";
        var expected = "Avengers Age Of Ultron";

        var result = template.ToTitleCase();

        Assert.Equal(expected, result);
    }
}
