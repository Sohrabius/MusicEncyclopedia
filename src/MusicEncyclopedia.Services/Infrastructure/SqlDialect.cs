namespace MusicEncyclopedia.Services.Infrastructure;

/// <summary>
/// Helper methods for writing SQL Server SQL.
/// </summary>
public static class SqlDialect
{
    /// <summary>
    /// Returns a SQL expression for LEFT(string, length) (SQL Server syntax).
    /// </summary>
    public static string Left(string column, int length)
    {
        return $"LEFT({column}, {length})";
    }

    /// <summary>
    /// Returns a SQL concatenation expression using the || operator,
    /// which works on SQL Server 2012+.
    /// </summary>
    public static string Concat(params string[] parts)
    {
        return string.Join(" || ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    /// <summary>
    /// Returns a CAST(column AS TEXT) expression for string coercion.
    /// </summary>
    public static string CastToText(string column)
    {
        return $"CAST({column} AS TEXT)";
    }

    /// <summary>
    /// Returns a random-ordering expression for SQL Server (NEWID()).
    /// </summary>
    public static string RandomOrder()
    {
        return "NEWID()";
    }

    /// <summary>
    /// Returns a pagination clause for SQL Server (OFFSET/FETCH NEXT).
    /// </summary>
    /// <param name="offsetParam">Name of the offset parameter.</param>
    /// <param name="pageSizeParam">Name of the page-size parameter.</param>
    public static string Pagination(
        string offsetParam = "@Offset",
        string pageSizeParam = "@PageSize")
    {
        return $"OFFSET {offsetParam} ROWS FETCH NEXT {pageSizeParam} ROWS ONLY";
    }
}
