namespace MusicEncyclopedia.Web.ViewModels;

/// <summary>
/// View model for error pages.
/// </summary>
public sealed class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId { get; set; }
    public int StatusCode { get; set; } = 500;
    public string Title { get; set; } = "Error";
    public string Message { get; set; } = "An unexpected error occurred while processing your request.";
}
