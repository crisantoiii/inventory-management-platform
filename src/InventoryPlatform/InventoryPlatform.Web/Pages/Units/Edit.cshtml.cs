using InventoryPlatform.Application.Features.Units.GetUnit;
using InventoryPlatform.Application.Features.Units.UpdateUnit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Units;

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