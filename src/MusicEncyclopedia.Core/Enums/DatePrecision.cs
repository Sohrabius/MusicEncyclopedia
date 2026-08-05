namespace MusicEncyclopedia.Core.Enums;

/// <summary>
/// Defines the precision level for a date value.
/// </summary>
public enum DatePrecision
{
    /// <summary>Exact full date (year, month, day).</summary>
    Exact = 0,

    /// <summary>Year and month only.</summary>
    YearMonth = 1,

    /// <summary>Year only.</summary>
    Year = 2,

    /// <summary>A range of years.</summary>
    YearRange = 3,

    /// <summary>A specific decade.</summary>
    Decade = 4,

    /// <summary>A specific century.</summary>
    Century = 5
}
