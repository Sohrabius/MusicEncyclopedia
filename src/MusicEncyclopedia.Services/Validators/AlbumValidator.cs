using FluentValidation;

namespace MusicEncyclopedia.Services.Validators;

/// <summary>
/// Validates album admin form input.
/// </summary>
public sealed class AlbumValidator : AbstractValidator<AlbumFormModel>
{
    public AlbumValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(255).WithMessage("Slug must not exceed 255 characters.");

        RuleFor(x => x.ReleaseDatePrecision)
            .Must(BeValidPrecision).When(x => !string.IsNullOrWhiteSpace(x.ReleaseDatePrecision))
            .WithMessage("ReleaseDatePrecision must be a valid precision value (day, month, year).");

        RuleFor(x => x.DurationSeconds)
            .GreaterThanOrEqualTo(0).When(x => x.DurationSeconds.HasValue)
            .WithMessage("DurationSeconds must be 0 or greater.");
    }

    private static bool BeValidPrecision(string? precision)
    {
        return precision is null or "day" or "month" or "year";
    }
}

/// <summary>
/// Represents the album admin form model for validation purposes.
/// </summary>
public sealed record AlbumFormModel
{
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? ReleaseDatePrecision { get; init; }
    public int? DurationSeconds { get; init; }
    // Additional fields can be added as needed
}
