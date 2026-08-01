using InventoryPlatform.Application.DTOs.Role;
using InventoryPlatform.Application.Features.Users.UpdateUserRoles;
using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.GetRoles;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using static InventoryPlatform.Infrastructure.Identity.IdentityConstants;

namespace InventoryPlatform.Web.Pages.Administrator.Users;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class EditRolesModel : PageModel
{
    private readonly UpdateUserRolesHandler _updateRolehandler;
    private readonly GetRolesHandler _getRolesHandler;
    private readonly GetUserHandler _getUserHandler;

    public EditRolesModel(
        UpdateUserRolesHandler updateRolehandler, 
        GetRolesHandler getRolesHandler, 
        GetUserHandler getUserHandler)
    {
        _updateRolehandler = updateRolehandler;
        _getRolesHandler = getRolesHandler;
        _getUserHandler = getUserHandler;
    }

    [BindProperty]
    public UpdateUserRolesRequest Input { get; set; } = new();

    public IList<RoleOption> RoleOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Update User Roles";

        var user = await _getUserHandler.HandleAsync(id);

        await LoadRolesAsync(cancellationToken);

        var userRolesSet = new HashSet<string>(user.Value.Roles, StringComparer.OrdinalIgnoreCase);

        RoleOptions = RoleOptions.Select(role => new RoleOption(
            role.Name,                                 
            userRolesSet.Contains(role.Name)           
        )).ToList();

        Input = new UpdateUserRolesRequest
        {
            Id = id,
            Roles = user.Value.Roles.ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(cancellationToken);

            return Page();
        }

        var result = await _updateRolehandler.HandleAsync(Input, cancellationToken);

        if (!result.IsSuccess)
        {
            await LoadRolesAsync(cancellationToken);

            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);
            
            return Page();
        }

        TempData["SuccessMessage"] =
            $"User Role '{Input.Id}' was update successfully.";

        return RedirectToPage(
            "./Details",
            new { id = Input.Id });
    }

    private async Task LoadRolesAsync(
    CancellationToken cancellationToken)
    {
        RoleOptions = (
            await _getRolesHandler.HandleAsync(cancellationToken))
            .ToList();
    }
}