using InventoryPlatform.Application.Features.Account.DisableTwoFactor;
using InventoryPlatform.Application.Features.Account.GetTwoFactorStatus;
using InventoryPlatform.Application.Features.Account.RegenerateTwoFactorRecoveryCodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class TwoFactorAuthenticationModel : PageModel
{
    private readonly GetTwoFactorStatusHandler _getTwoFactorStatusHandler;
    private readonly DisableTwoFactorHandler _disableTwoFactorHandler;
    private readonly RegenerateTwoFactorRecoveryCodesHandler
    _regenerateTwoFactorRecoveryCodesHandler;

    public TwoFactorAuthenticationModel(
        GetTwoFactorStatusHandler getTwoFactorStatusHandler,
        DisableTwoFactorHandler disableTwoFactorHandler,
        RegenerateTwoFactorRecoveryCodesHandler
            regenerateTwoFactorRecoveryCodesHandler)
    {
        _getTwoFactorStatusHandler = getTwoFactorStatusHandler;
        _disableTwoFactorHandler = disableTwoFactorHandler;
        _regenerateTwoFactorRecoveryCodesHandler =
            regenerateTwoFactorRecoveryCodesHandler;
    }

    public bool TwoFactorEnabled { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var result = await _getTwoFactorStatusHandler.HandleAsync(
            new GetTwoFactorStatusRequest
            {
                UserId = userId
            },
            cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        TwoFactorEnabled = result.Value.TwoFactorEnabled;

        return Page();
    }

    public async Task<IActionResult> OnPostDisableAsync(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var result = await _disableTwoFactorHandler.HandleAsync(
            new DisableTwoFactorRequest
            {
                UserId = userId
            },
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            await LoadStatusAsync(cancellationToken);

            return Page();
        }

        return RedirectToPage();
    }

    private async Task LoadStatusAsync(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var result = await _getTwoFactorStatusHandler.HandleAsync(
            new GetTwoFactorStatusRequest
            {
                UserId = userId
            },
            cancellationToken);

        if (result.IsSuccess)
        {
            TwoFactorEnabled = result.Value.TwoFactorEnabled;
        }
    }

    public async Task<IActionResult> OnPostRegenerateRecoveryCodesAsync(
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var result =
            await _regenerateTwoFactorRecoveryCodesHandler.HandleAsync(
                new RegenerateTwoFactorRecoveryCodesRequest
                {
                    UserId = userId
                },
                cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            await LoadStatusAsync(cancellationToken);

            return Page();
        }

        TempData["TwoFactorRecoveryCodes"] =
            result.Value.RecoveryCodes.ToArray();

        return RedirectToPage(
            "/Account/TwoFactorRecoveryCodes");
    }
}