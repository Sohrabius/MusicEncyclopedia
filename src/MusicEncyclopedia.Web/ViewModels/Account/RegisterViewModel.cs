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
    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "آدرس ایمیل نامعتبر است.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [StringLength(100, ErrorMessage = "رمز عبور باید حداقل {2} و حداکثر {1} کاراکتر باشد.", MinimumLength = 10)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of the password.
    /// </summary>
    [Required(ErrorMessage = "تأیید رمز عبور الزامی است.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "رمز عبور و تأیید آن یکسان نیستند.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// The URL to redirect to after successful registration.
    /// </summary>
    [HiddenInput]
    public string? ReturnUrl { get; set; }
}
