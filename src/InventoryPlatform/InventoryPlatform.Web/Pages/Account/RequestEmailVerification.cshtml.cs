using InventoryPlatform.Application.Features.Account.RequestEmailVerification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class RequestEmailVerificationModel : PageModel
{
    private readonly RequestEmailVerificationHandler _handler;

    public RequestEmailVerificationModel(
        RequestEmailVerificationHandler handler)
    {
        _handler = handler;
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

        var request = new RequestEmailVerificationRequest
        {
            UserId = userId
        };

        var result = await _handler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
        {
            TempData["ErrorMessage"] =
                result.Error.Message;

            return RedirectToPage("/Account/Profile");
        }

        TempData["StatusMessage"] = result.Value.AlreadyVerified
            ? "Your email address is already verified."
            : "A verification email has been sent to your email address.";

        return RedirectToPage("/Account/Profile");
    }
}