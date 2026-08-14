using System.Security.Claims;
using InventoryPlatform.Application.Features.Account.SetupTwoFactor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class TwoFactorSetupModel : PageModel
{
    private readonly SetupTwoFactorHandler _setupTwoFactorHandler;

    public TwoFactorSetupModel(
        SetupTwoFactorHandler setupTwoFactorHandler)
    {
        _setupTwoFactorHandler = setupTwoFactorHandler;
    }

    public string AuthenticatorKey { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var result = await _setupTwoFactorHandler.HandleAsync(
            new SetupTwoFactorRequest
            {
                UserId = userId
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        AuthenticatorKey = result.Value.AuthenticatorKey;

        return Page();
    }
}