using MusicEncyclopedia.Services.Validators;

namespace MusicEncyclopedia.Services.Tests;

public sealed class ValidatorTests
{
    [Fact]
    public void AlbumValidator_AcceptsValidModel()
    {
        var result = new AlbumValidator().Validate(new AlbumFormModel
        {
            Title = "Kind of Blue",
            Slug = "kind-of-blue",
            ReleaseDatePrecision = "day",
            DurationSeconds = 2733
        });

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("century", 10)]
    [InlineData("day", -1)]
    public void AlbumValidator_RejectsInvalidPrecisionOrDuration(string precision, int duration)
    {
        var result = new AlbumValidator().Validate(new AlbumFormModel
        {
            Title = "Album",
            Slug = "album",
            ReleaseDatePrecision = precision,
            DurationSeconds = duration
        });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TrackValidator_RejectsMalformedIsrcAndOutOfRangeValues()
    {
        var result = new TrackValidator().Validate(new TrackFormModel
        {
            Title = "Track",
            Isrc = "short",
            Bpm = 1001,
            DurationSeconds = -1
        });

        result.Errors.Select(error => error.PropertyName)
            .Should().BeEquivalentTo(["Isrc", "Bpm", "DurationSeconds"]);
    }

    [Theory]
    [InlineData(1, null, null, true)]
    [InlineData(null, 2, null, true)]
    [InlineData(1, null, 3, true)]
    [InlineData(null, null, null, false)]
    [InlineData(1, 2, null, false)]
    [InlineData(null, 2, 3, false)]
    public void CreditValidator_RequiresExactlyOneContributorAndPersonForInstrument(
        int? personId,
        int? companyId,
        int? instrumentId,
        bool expectedValid)
    {
        var result = new CreditValidator().Validate(new CreditFormModel
        {
            PersonId = personId,
            CompanyId = companyId,
            InstrumentId = instrumentId,
            DisplayOrder = 0
        });

        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void CreditValidator_RejectsNegativeDisplayOrder()
    {
        var result = new CreditValidator().Validate(new CreditFormModel
        {
            PersonId = 1,
            DisplayOrder = -1
        });

        result.Errors.Should().Contain(error => error.PropertyName == "DisplayOrder");
    }
}
