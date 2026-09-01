using InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroup;
using InventoryPlatform.Application.Features.AuthorizationGroups.UpdateAuthorizationGroup;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Groups;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class EditModel : PageModel
{
    private readonly GetAuthorizationGroupHandler _getHandler;
    private readonly UpdateAuthorizationGroupHandler _updateHandler;

    public EditModel(
        GetAuthorizationGroupHandler getHandler,
        UpdateAuthorizationGroupHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateAuthorizationGroupRequest Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Edit Authorization Group";

        var result = await _getHandler.HandleAsync(
            new GetAuthorizationGroupRequest(id),
            cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Input = new UpdateAuthorizationGroupRequest
        {
            Id = result.Value.Id,
            Name = result.Value.Name
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _updateHandler.HandleAsync(Input, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return Page();
        }

        TempData["SuccessMessage"] =
            $"Authorization group was updated successfully.";

        return RedirectToPage("Index");
    }
}
