namespace MusicEncyclopedia.Services.Infrastructure;

/// <summary>
/// Helper methods for writing cross-dialect SQL that works
/// with both SQL Server and SQLite.
/// </summary>
public static class SqlDialect
{
    /// <summary>
    /// Returns a SQL expression for LEFT(string, length) that works
    /// on both SQL Server (LEFT) and SQLite (SUBSTR).
    /// </summary>
    public static string Left(string column, int length, bool isSqlite)
    {
        return isSqlite ? $"SUBSTR({column}, 1, {length})" : $"LEFT({column}, {length})";
    }

    /// <summary>
    /// Returns a SQL concatenation expression using the || operator,
    /// which works on both SQL Server (2012+) and SQLite.
    /// </summary>
    public static string Concat(params string[] parts)
    {
        return string.Join(" || ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    /// <summary>
    /// Returns a CAST(column AS TEXT) expression for cross-dialect
    /// string coercion.
    /// </summary>
    public static string CastToText(string column)
    {
        return $"CAST({column} AS TEXT)";
    }
}
