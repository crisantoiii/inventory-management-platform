using InventoryPlatform.Application.DTOs.Role;
using InventoryPlatform.Application.Features.Users.CreateUser;
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
public class CreateModel : PageModel
{
    private readonly CreateUserHandler _handler;
    private readonly GetRolesHandler _roleHandler;

    public CreateModel(CreateUserHandler handler, GetRolesHandler roleHandler)
    {
        _handler = handler;
        _roleHandler = roleHandler;
    }

    [BindProperty]
    public CreateUserRequest Input { get; set; } = new();

    public IList<RoleOption> RoleOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Create New User";

        await LoadRolesAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(cancellationToken);

            return Page();
        }

        var result = await _handler.HandleAsync(Input, cancellationToken);
        if (!result.IsSuccess)
        {
            await LoadRolesAsync(cancellationToken);

            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);
            
            return Page();
        }

        TempData["SuccessMessage"] =
            $"User '{result.Value!}' was created successfully.";

        return RedirectToPage("Index");
    }

    private async Task LoadRolesAsync(
    CancellationToken cancellationToken)
    {
        RoleOptions = (
            await _roleHandler.HandleAsync(cancellationToken))
            .ToList();
    }
}