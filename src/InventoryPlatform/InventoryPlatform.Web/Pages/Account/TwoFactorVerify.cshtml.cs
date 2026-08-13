using System.Security.Claims;
using InventoryPlatform.Application.Features.Account.VerifyTwoFactor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class TwoFactorVerifyModel : PageModel
{
    private readonly VerifyTwoFactorHandler _verifyTwoFactorHandler;

    public TwoFactorVerifyModel(
        VerifyTwoFactorHandler verifyTwoFactorHandler)
    {
        _verifyTwoFactorHandler = verifyTwoFactorHandler;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public string Code { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ModelState.AddModelError(
                nameof(Input.Code),
                "Enter the verification code.");

            return Page();
        }

        var result = await _verifyTwoFactorHandler.HandleAsync(
            new VerifyTwoFactorRequest
            {
                UserId = userId,
                Code = Input.Code.Trim()
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["TwoFactorRecoveryCodes"] =
            result.Value.RecoveryCodes.ToArray();

        return RedirectToPage(
            "/Account/TwoFactorRecoveryCodes");
    }
}