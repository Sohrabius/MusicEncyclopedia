using MusicEncyclopedia.Services.Services;

namespace MusicEncyclopedia.Services.Tests;

public sealed class SlugServiceTests
{
    private readonly SlugService _service = new();

    [Theory]
    [InlineData("Kind of Blue", "kind-of-blue")]
    [InlineData("  Kind__of---Blue!  ", "kind-of-blue")]
    [InlineData("موسیقی ایرانی", "موسیقی-ایرانی")]
    public void GenerateSlug_NormalizesTextAndPreservesPersian(string input, string expected)
    {
        _service.GenerateSlug(input).Should().Be(expected);
        _service.IsValidSlug(expected).Should().BeTrue();
    }

    [Fact]
    public void GenerateSlug_TruncatesAtAValidBoundary()
    {
        var result = _service.GenerateSlug(new string('a', 254) + "-tail");

        result.Should().HaveLength(254);
        result.Should().NotEndWith("-");
        _service.IsValidSlug(result).Should().BeTrue();
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("API")]
    [InlineData("robots.txt")]
    public void IsReservedRoute_IsCaseInsensitive(string route)
    {
        _service.IsReservedRoute(route).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("-leading")]
    [InlineData("contains space")]
    [InlineData("café")]
    public void IsValidSlug_RejectsInvalidValues(string slug)
    {
        _service.IsValidSlug(slug).Should().BeFalse();
    }
}
