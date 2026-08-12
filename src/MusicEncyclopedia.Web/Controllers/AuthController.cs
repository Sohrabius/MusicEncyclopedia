using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MusicEncyclopedia.Web.ViewModels.Account;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Handles authentication: login, logout, registration, access-denied, and forgot-password.
/// Route is NOT culture-prefixed — these endpoints are language-neutral.
/// </summary>
[Route("auth")]
public sealed class AuthController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AuthController> _logger;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AuthController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ILogger<AuthController> logger,
        IStringLocalizer<SharedResources> localizer)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _localizer = localizer;
    }

    // ──────────────────────────────────────────────
    //  GET /auth/login
    // ──────────────────────────────────────────────
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = _localizer["Login"];

        // If the user is already authenticated, redirect away
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    // ──────────────────────────────────────────────
    //  POST /auth/login
    // ──────────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["Title"] = _localizer["Login"];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Attempt to sign in
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            // Two-factor is not yet implemented; redirect to login with an error
            ModelState.AddModelError(string.Empty, _localizer["Two-factor authentication is required but not yet configured."]);
            return View(model);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} is locked out.", model.Email);
            ModelState.AddModelError(string.Empty, _localizer["This account has been locked out due to too many failed attempts. Please try again in 15 minutes."]);
            return View(model);
        }

        if (result.IsNotAllowed)
        {
            _logger.LogWarning("User {Email} is not allowed to sign in (email not confirmed or account disabled).", model.Email);
            ModelState.AddModelError(string.Empty, _localizer["Sign in is not allowed. Please confirm your email or contact support."]);
            return View(model);
        }

        // Default failure
        _logger.LogWarning("Failed login attempt for {Email}.", model.Email);
        ModelState.AddModelError(string.Empty, _localizer["Invalid login attempt. Please check your email and password."]);
        return View(model);
    }

    // ──────────────────────────────────────────────
    //  POST /auth/logout
    // ──────────────────────────────────────────────
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name ?? "unknown";
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserName} logged out.", userName);

        return RedirectToAction("Index", "Home", new { culture = GetCulture() });
    }

    // ──────────────────────────────────────────────
    //  GET /auth/register
    // ──────────────────────────────────────────────
    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["Title"] = _localizer["Register"];

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new RegisterViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    // ──────────────────────────────────────────────
    //  POST /auth/register
    // ──────────────────────────────────────────────
    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewData["Title"] = _localizer["Register"];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} created a new account.", model.Email);

            // Sign in the user automatically after registration
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User {Email} signed in after registration.", model.Email);

            return RedirectToLocal(model.ReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
            _logger.LogWarning("Registration error for {Email}: {Error}", model.Email, error.Description);
        }

        return View(model);
    }

    // ──────────────────────────────────────────────
    //  GET /auth/access-denied
    // ──────────────────────────────────────────────
    [HttpGet("access-denied")]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = _localizer["Access Denied"];
        return View();
    }

    // ──────────────────────────────────────────────
    //  GET /auth/forgot-password
    // ──────────────────────────────────────────────
    [HttpGet("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = _localizer["Forgot Password"];
        return View();
    }

    // ──────────────────────────────────────────────
    //  POST /auth/forgot-password
    // ──────────────────────────────────────────────
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        ViewData["Title"] = _localizer["Forgot Password"];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Look up the user by email
        var user = await _userManager.FindByEmailAsync(model.Email);

        // Always return a success message to prevent email enumeration
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            _logger.LogInformation("Forgot-password requested for non-existent or unconfirmed email: {Email}", model.Email);
            // Still show success to avoid revealing which accounts exist
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // Generate password reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Password reset token generated for {Email}.", model.Email);

        // In a real application, send the token via email.
        // For now, log it and show the confirmation page.
        _logger.LogInformation("Password reset token for {Email}: {Token}", model.Email, token);

        return RedirectToAction("ForgotPasswordConfirmation");
    }

    // ──────────────────────────────────────────────
    //  GET /auth/forgot-password-confirmation
    // ──────────────────────────────────────────────
    [HttpGet("forgot-password-confirmation")]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        ViewData["Title"] = _localizer["Forgot Password Confirmation"];
        return View();
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Redirects to the local return URL if it is local, otherwise to the home page.
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home", new { culture = GetCulture() });
    }

    /// <summary>
    /// Extracts the current culture from route data or defaults to "fa".
    /// </summary>
    private string GetCulture()
    {
        return HttpContext.GetRouteValue("culture") as string
               ?? HttpContext.Items["culture"] as string
               ?? "fa";
    }
}
