using InventoryPlatform.Application.DTOs.Role;
using InventoryPlatform.Application.Features.Users.GetRoles;
using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.UpdateUserRoles;
using InventoryPlatform.Application.Features.Users.UpdateUserStatus;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

using customRoleIdentity =  InventoryPlatform.Infrastructure.Identity;

namespace InventoryPlatform.Web.Pages.Administrator.Users;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class EditStatusModel : PageModel
{
    private readonly UpdateUserStatusHandler _updateStatushandler;
    private readonly GetUserHandler _getUserHandler;

    public EditStatusModel(
        UpdateUserStatusHandler getRolesHandler, 
        GetUserHandler getUserHandler)
    {
        _updateStatushandler = getRolesHandler;
        _getUserHandler = getUserHandler;
    }

    [BindProperty]
    public UpdateUserStatusRequest Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Update User Roles";

        var user = await _getUserHandler.HandleAsync(id);

        Input = new UpdateUserStatusRequest
        {
            Id = id,
            IsActive = !user.Value.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _getUserHandler.HandleAsync(Input.Id);
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (User.IsInRole(customRoleIdentity.IdentityConstants.Roles.InventoryManager))
        {
            if (user.Value.Id.ToString() == currentUserId)
            {
                return Page();
            }
        }

        Input = new UpdateUserStatusRequest
        {
            Id = user.Value.Id,
            IsActive = !user.Value.IsActive
        };

        var result = await _updateStatushandler.HandleAsync(Input, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);
            
            return Page();
        }

        TempData["SuccessMessage"] =
            $"User Status '{Input.Id}' was update successfully.";

        return RedirectToPage(
            "./Details",
            new { id = Input.Id });
    }

}