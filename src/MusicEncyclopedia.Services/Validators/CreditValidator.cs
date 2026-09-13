using FluentValidation;

namespace MusicEncyclopedia.Services.Validators;

/// <summary>
/// Validates credit editor input.
/// </summary>
public sealed class CreditValidator : AbstractValidator<CreditFormModel>
{
    public CreditValidator()
    {
        // A credit belongs to exactly one contributor. Accepting both creates an
        // ambiguous row that public queries cannot display consistently.
        RuleFor(x => x)
            .Must(x => x.PersonId.HasValue ^ x.CompanyId.HasValue)
            .WithMessage("Credit must have either a Person or a Company, but not both.");

        // If InstrumentId is present, PersonId must be present
        RuleFor(x => x.PersonId)
            .NotNull()
            .When(x => x.InstrumentId.HasValue)
            .WithMessage("When an instrument is specified, a person must be selected.");

        // DisplayOrder must be >= 0
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder must be 0 or greater.");
    }
}

/// <summary>
/// Represents the credit form model for validation purposes.
/// </summary>
public sealed record CreditFormModel
{
    public int? PersonId { get; init; }
    public int? CompanyId { get; init; }
    public int? InstrumentId { get; init; }
    public int DisplayOrder { get; init; }
}
