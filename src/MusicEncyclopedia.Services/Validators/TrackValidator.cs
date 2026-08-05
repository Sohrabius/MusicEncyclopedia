using FluentValidation;

namespace MusicEncyclopedia.Services.Validators;

/// <summary>
/// Validates track admin form input.
/// </summary>
public sealed class TrackValidator : AbstractValidator<TrackFormModel>
{
    public TrackValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");

        RuleFor(x => x.Isrc)
            .Length(12).When(x => !string.IsNullOrWhiteSpace(x.Isrc))
            .WithMessage("ISRC must be exactly 12 characters.");

        RuleFor(x => x.Bpm)
            .InclusiveBetween((short)0, (short)1000).When(x => x.Bpm.HasValue)
            .WithMessage("BPM must be between 0 and 1000.");

        RuleFor(x => x.DurationSeconds)
            .GreaterThanOrEqualTo(0).When(x => x.DurationSeconds.HasValue)
            .WithMessage("DurationSeconds must be 0 or greater.");
    }
}

/// <summary>
/// Represents the track admin form model for validation purposes.
/// </summary>
public sealed record TrackFormModel
{
    public string Title { get; init; } = "";
    public string? Isrc { get; init; }
    public short? Bpm { get; init; }
    public int? DurationSeconds { get; init; }
}
