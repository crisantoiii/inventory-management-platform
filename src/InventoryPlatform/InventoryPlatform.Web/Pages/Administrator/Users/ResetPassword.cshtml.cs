using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.ResetPassowrd;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Administrator.Users;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class ResetPasswordModel : PageModel
{
    private readonly ResetPasswordHandler _resetPasswordHandler;
    private readonly GetUserHandler _getUserHandler;

    public ResetPasswordModel(
        ResetPasswordHandler resetPasswordHandler, 
        GetUserHandler getUserHandler)
    {
        _resetPasswordHandler = resetPasswordHandler;
        _getUserHandler = getUserHandler;
    }

    [BindProperty]
    public ResetPasswordRequest Input { get; set; } = new();

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Reset Password";
        var user = await _getUserHandler.HandleAsync(id);

        UserName = user.Value.Username;
        Email = user.Value.Email;

        Input = new ResetPasswordRequest
        {
            Id = id,
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _resetPasswordHandler.HandleAsync(Input, cancellationToken);
        if (!result.IsSuccess)
        {
            var user = await _getUserHandler.HandleAsync(Input.Id);

            UserName = user.Value.Username;
            Email = user.Value.Email;

            return Page();
        }

        TempData["SuccessMessage"] =
            $"User '{Input.Id}' password was change successfully.";

        return RedirectToPage("Index");
    }

}