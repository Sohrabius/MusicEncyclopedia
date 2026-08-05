using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Web.ViewModels.Api;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Base class for all API v1 controllers.
/// Provides consistent response helpers and error handling.
/// </summary>
[ApiController]
[Route("/api/v1")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns a 200 OK with a single-item success envelope.
    /// </summary>
    [NonAction]
    protected IActionResult OkResult<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    /// <summary>
    /// Returns a 200 OK with a paginated list success envelope.
    /// </summary>
    [NonAction]
    protected IActionResult OkListResult<T>(PagedResult<T> paged) =>
        Ok(ApiResponse<ApiListResponse<T>>.Ok(ApiListResponse<T>.FromPagedResult(paged)));

    /// <summary>
    /// Returns a 200 OK with a manual list envelope (used when items come from Dapper directly).
    /// </summary>
    [NonAction]
    protected IActionResult OkListResult<T>(List<T> items, int page, int pageSize, int totalItems, int totalPages) =>
        Ok(ApiResponse<ApiListResponse<T>>.Ok(new ApiListResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        }));

    /// <summary>
    /// Returns a 404 Not Found with an error message.
    /// </summary>
    [NonAction]
    protected IActionResult NotFoundResult(string message = "Resource not found.") =>
        NotFound(ApiResponse<object>.Fail(message));

    /// <summary>
    /// Returns a 400 Bad Request with validation errors.
    /// </summary>
    [NonAction]
    protected IActionResult BadRequestResult(string message, List<ApiError>? errors = null) =>
        BadRequest(ApiResponse<object>.Fail(message, errors));

    /// <summary>
    /// Returns a 400 Bad Request with a single validation error.
    /// </summary>
    [NonAction]
    protected IActionResult BadRequestResult(string field, string code, string message) =>
        BadRequest(ApiResponse<object>.Fail("Validation failed.", new List<ApiError>
        {
            new() { Field = field, Code = code, Message = message }
        }));

    /// <summary>
    /// Returns a 200 OK with a list of items (non-paginated).
    /// </summary>
    [NonAction]
    protected IActionResult OkListResult<T>(IReadOnlyList<T> items) =>
        Ok(ApiResponse<List<T>>.Ok([.. items]));
}
