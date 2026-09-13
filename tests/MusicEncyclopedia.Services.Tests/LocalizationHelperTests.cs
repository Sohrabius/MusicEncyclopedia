using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Services.Infrastructure;

namespace MusicEncyclopedia.Services.Tests;

public sealed class LocalizationHelperTests
{
    [Fact]
    public void Pick_UsesRequestedValueBeforeBaseAndFallback()
    {
        var fields = Fields(requested: "عنوان فارسی", fallback: "English title");

        LocalizationHelper.Pick(fields, "Title", "Base title").Should().Be("عنوان فارسی");
    }

    [Fact]
    public void Pick_UsesBaseWhenRequestedValueIsMissing()
    {
        var fields = Fields(requested: null, fallback: "English title");

        LocalizationHelper.Pick(fields, "Title", "Base title").Should().Be("Base title");
    }

    [Fact]
    public void Pick_UsesEnglishFallbackOnlyWhenRequestedAndBaseAreMissing()
    {
        var fields = Fields(requested: " ", fallback: "English title");

        LocalizationHelper.Pick(fields, "Title", null).Should().Be("English title");
    }

    [Fact]
    public void Pick_ReturnsEmptyWhenNoValueExists()
    {
        LocalizationHelper.Pick(
            new Dictionary<string, LocalizedFieldValues>(), "Title", null).Should().BeEmpty();
    }

    [Theory]
    [InlineData("fa", true)]
    [InlineData("ar", true)]
    [InlineData("fr", true)]
    [InlineData("en", false)]
    [InlineData("EN", false)]
    [InlineData(null, false)]
    public void ShouldLocalize_OnlyOverlaysNonBaseCultures(string? culture, bool expected)
    {
        LocalizationHelper.ShouldLocalize(culture).Should().Be(expected);
    }

    private static IReadOnlyDictionary<string, LocalizedFieldValues> Fields(
        string? requested,
        string? fallback) => new Dictionary<string, LocalizedFieldValues>
        {
            ["Title"] = new(requested, fallback)
        };
}
