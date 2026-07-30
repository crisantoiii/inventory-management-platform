using InventoryPlatform.Application.Features.Units.GetUnit;
using InventoryPlatform.Application.Features.Units.UpdateUnit;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Units;

[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]
public class EditModel : PageModel
{
    private readonly GetUnitHandler _getHandler;
    private readonly UpdateUnitHandler _updateHandler;

    public EditModel(
        GetUnitHandler getHandler,
        UpdateUnitHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateUnitRequest Unit { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Edit Unit";

        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        var category = result.Value;

        Unit = new UpdateUnitRequest(
            category.Id,
            category.Code,
            category.Name,
            category.Symbol);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _updateHandler.HandleAsync(Unit);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        SuccessMessage = $"Unit '{result.Value!.Name}' updated successfully.";

        return RedirectToPage("Index");
    }
}