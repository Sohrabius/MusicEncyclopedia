namespace MusicEncyclopedia.Web.ViewModels.Api;

/// <summary>
/// Represents a single API error detail.
/// Spec 11.3 — error item within the response envelope.
/// </summary>
public sealed class ApiError
{
    /// <summary>The field name that caused the error, if applicable.</summary>
    public string? Field { get; init; }

    /// <summary>A machine-readable error code.</summary>
    public string? Code { get; init; }

    /// <summary>A human-readable error message.</summary>
    public string? Message { get; init; }
}

/// <summary>
/// Generic API response envelope for single-item responses.
/// Spec 11.3 — all API responses use this shape.
/// </summary>
/// <typeparam name="T">The type of the payload.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>Indicates whether the request succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>The response payload (null on failure).</summary>
    public T? Data { get; set; }

    /// <summary>An optional human-readable message.</summary>
    public string? Message { get; set; }

    /// <summary>Validation or business-rule errors.</summary>
    public List<ApiError> Errors { get; set; } = [];

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    public static ApiResponse<T> Fail(string message, List<ApiError>? errors = null) =>
        new() { Success = false, Data = default, Message = message, Errors = errors ?? [] };
}

/// <summary>
/// Paginated list envelope embedded inside <see cref="ApiResponse{T}"/>.
/// Spec 11.3 — list responses wrap items in a data envelope with pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class ApiListResponse<T>
{
    /// <summary>The items for the current page.</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; set; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalItems { get; set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Creates an <see cref="ApiListResponse{T}"/> from a <see cref="PagedResult{T}"/>.
    /// </summary>
    public static ApiListResponse<T> FromPagedResult(Core.DTOs.PagedResult<T> paged) =>
        new()
        {
            Items = [.. paged.Items],
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages
        };
}
