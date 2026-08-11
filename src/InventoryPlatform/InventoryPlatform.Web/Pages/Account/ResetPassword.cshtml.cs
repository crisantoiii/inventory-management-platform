using System.ComponentModel.DataAnnotations;
using InventoryPlatform.Application.Features.Account.ResetPassword;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class ResetPasswordModel : PageModel
{
    private readonly ResetPasswordHandler _resetPasswordHandler;

    public ResetPasswordModel(
        ResetPasswordHandler resetPasswordHandler)
    {
        _resetPasswordHandler = resetPasswordHandler;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool ResetSucceeded { get; private set; }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

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

    public void OnGet(
        string? email,
        string? token)
    {
        Input.Email = email ?? string.Empty;
        Input.Token = token ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new ResetPasswordRequest
        {
            Email = Input.Email.Trim(),
            Token = Input.Token,
            NewPassword = Input.NewPassword
        };

        var result = await _resetPasswordHandler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        ResetSucceeded = true;

        return Page();
    }
}