using System.ComponentModel.DataAnnotations;

namespace MusicEncyclopedia.Web.ViewModels.Account;

/// <summary>
/// View model for the forgot-password form.
/// </summary>
public sealed class ForgotPasswordViewModel
{
    /// <summary>
    /// The email address of the account to reset the password for.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
