using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MusicEncyclopedia.Web.ViewModels.Account;

/// <summary>
/// View model for the registration form.
/// </summary>
public sealed class RegisterViewModel
{
    /// <summary>
    /// The user's email address (used as the username).
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 10)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of the password.
    /// </summary>
    [Required(ErrorMessage = "Confirm password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// The URL to redirect to after successful registration.
    /// </summary>
    [HiddenInput]
    public string? ReturnUrl { get; set; }
}
