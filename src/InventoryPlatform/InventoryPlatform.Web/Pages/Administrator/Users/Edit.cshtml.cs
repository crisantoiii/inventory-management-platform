using InventoryPlatform.Application.Features.Categories.UpdateCategory;
using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.UpdateUser;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Administrator.Users;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class EditModel : PageModel
{
    private readonly GetUserHandler _getHandler;
    private readonly UpdateUserHandler _updateHandler;

    public EditModel(GetUserHandler getHandler,
        UpdateUserHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateUserRequest User { get; set; } = new();


    public async Task<IActionResult> OnGetAsync( Guid id ,CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Create Update User";

        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        var user = result.Value;

        User = new UpdateUserRequest
        {
            Id = user.Id,
            UserName = user.Username!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _updateHandler.HandleAsync(
            User,
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"User '{User.UserName}' was updated successfully.";

        return RedirectToPage(
            "./Details",
            new { id = User.Id });
    }
}