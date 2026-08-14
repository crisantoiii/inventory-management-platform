using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class TwoFactorRecoveryCodesModel : PageModel
{
    public IReadOnlyList<string> RecoveryCodes { get; private set; } =
        Array.Empty<string>();

    public IActionResult OnGet()
    {
        if (TempData["TwoFactorRecoveryCodes"] is not string[] recoveryCodes ||
            recoveryCodes.Length == 0)
        {
            return RedirectToPage(
                "/Account/TwoFactorAuthentication");
        }

        RecoveryCodes = recoveryCodes;

        return Page();
    }
}