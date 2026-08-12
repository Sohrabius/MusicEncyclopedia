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
    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "آدرس ایمیل نامعتبر است.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required(ErrorMessage = "رمز عبور الزامی است.")]
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
