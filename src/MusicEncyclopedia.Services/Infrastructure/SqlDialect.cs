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

    /// <summary>
    /// Returns true when <paramref name="connection"/> is a SQLite connection.
    /// The registered <see cref="System.Data.IDbConnection"/> type is chosen
    /// in Program.cs based on the configured provider, so the concrete type
    /// reliably identifies the dialect.
    /// </summary>
    public static bool IsSqliteConnection(System.Data.IDbConnection connection)
    {
        return connection is not null && connection.GetType().Name == "SqliteConnection";
    }

    /// <summary>
    /// Returns a pagination clause for the given dialect:
    /// SQL Server uses OFFSET/FETCH NEXT, SQLite uses LIMIT/OFFSET.
    /// </summary>
    /// <param name="isSqlite">True for SQLite, false for SQL Server.</param>
    /// <param name="offsetParam">Name of the offset parameter.</param>
    /// <param name="pageSizeParam">Name of the page-size parameter.</param>
    public static string Pagination(
        bool isSqlite,
        string offsetParam = "@Offset",
        string pageSizeParam = "@PageSize")
    {
        return isSqlite
            ? $"LIMIT {pageSizeParam} OFFSET {offsetParam}"
            : $"OFFSET {offsetParam} ROWS FETCH NEXT {pageSizeParam} ROWS ONLY";
    }
}
