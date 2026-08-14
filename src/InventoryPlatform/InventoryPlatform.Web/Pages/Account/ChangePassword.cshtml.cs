using InventoryPlatform.Application.Features.Account.ChangePassword;
using InventoryPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class ChangePasswordModel : PageModel
{
    private readonly ChangePasswordHandler _changePasswordHandler;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePasswordModel(
        ChangePasswordHandler changePasswordHandler,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _changePasswordHandler = changePasswordHandler;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
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

        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var request = new ChangePasswordRequest
        {
            UserId = userId,
            CurrentPassword = Input.CurrentPassword,
            NewPassword = Input.NewPassword
        };

        var result = await _changePasswordHandler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["StatusMessage"] =
            "Your password has been changed successfully.";

        return RedirectToPage("/Account/Profile");
    }
}