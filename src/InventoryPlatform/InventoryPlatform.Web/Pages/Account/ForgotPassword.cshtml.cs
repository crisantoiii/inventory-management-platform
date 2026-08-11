using System.ComponentModel.DataAnnotations;
using InventoryPlatform.Application.Features.Account.ForgotPassword;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class ForgotPasswordModel : PageModel
{
    private readonly ForgotPasswordHandler _forgotPasswordHandler;

    public ForgotPasswordModel(
        ForgotPasswordHandler forgotPasswordHandler)
    {
        _forgotPasswordHandler = forgotPasswordHandler;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Submitted { get; private set; }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new ForgotPasswordRequest
        {
            Email = Input.Email.Trim()
        };

        await _forgotPasswordHandler.HandleAsync(
            request,
            cancellationToken);

        // Always show the same message.
        // This prevents account enumeration.
        Submitted = true;

        return Page();
    }
}