using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MusicEncyclopedia.Web.ViewModels.Account;

/// <summary>
/// View model for the login form.
/// </summary>
public sealed class LoginViewModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to persist the authentication cookie across browser sessions.
    /// </summary>
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    /// <summary>
    /// The URL to redirect to after successful login.
    /// </summary>
    [HiddenInput]
    public string? ReturnUrl { get; set; }
}
