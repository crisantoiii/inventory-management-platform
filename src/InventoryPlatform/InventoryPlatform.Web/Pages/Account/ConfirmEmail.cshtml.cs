using InventoryPlatform.Application.Features.Account.ConfirmEmail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class ConfirmEmailModel : PageModel
{
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailModel(
        ConfirmEmailHandler handler)
    {
        _handler = handler;
    }

    public bool IsConfirmed { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        Guid? userId,
        string? token,
        CancellationToken cancellationToken)
    {
        if (!userId.HasValue || string.IsNullOrWhiteSpace(token))
        {
            ErrorMessage =
                "The email verification link is invalid.";

            return Page();
        }

        var request = new ConfirmEmailRequest
        {
            UserId = userId.Value,
            Token = token
        };

        var result = await _handler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;

            return Page();
        }

        IsConfirmed = true;

        return Page();
    }
}