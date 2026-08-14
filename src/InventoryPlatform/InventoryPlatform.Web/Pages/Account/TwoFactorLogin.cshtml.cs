using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventoryPlatform.Infrastructure.Identity;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class TwoFactorLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public TwoFactorLoginModel(
        SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public bool UseRecoveryCode { get; private set; }

    public sealed class InputModel
    {
        public string Code { get; set; } = string.Empty;

        public bool RememberMachine { get; set; }
    }

    public void OnGet(
        string? returnUrl = null,
        bool recoveryCode = false)
    {
        ReturnUrl = returnUrl;
        UseRecoveryCode = recoveryCode;
    }

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        ReturnUrl ??= Url.Content("~/Dashboard");

        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ModelState.AddModelError(
                nameof(Input.Code),
                "Enter the verification code.");

            return Page();
        }

        var authenticatorCode = Input.Code
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);

        var result =
            await _signInManager.TwoFactorAuthenticatorSignInAsync(
                authenticatorCode,
                Input.RememberMachine,
                rememberClient: Input.RememberMachine);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is locked out.");

            return Page();
        }

        ModelState.AddModelError(
            string.Empty,
            "The verification code is invalid.");

        return Page();
    }

    public async Task<IActionResult> OnPostRecoveryCodeAsync(
    string? returnUrl = null)
    {
        ReturnUrl ??= Url.Content("~/Dashboard");

        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ModelState.AddModelError(
                nameof(Input.Code),
                "Enter a recovery code.");

            UseRecoveryCode = true;
            return Page();
        }

        var recoveryCode = Input.Code.Trim();

        var result =
            await _signInManager.TwoFactorRecoveryCodeSignInAsync(
                recoveryCode);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is locked out.");

            UseRecoveryCode = true;
            return Page();
        }

        ModelState.AddModelError(
            string.Empty,
            "The recovery code is invalid.");

        UseRecoveryCode = true;

        return Page();
    }
}