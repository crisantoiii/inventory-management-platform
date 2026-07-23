using InventoryPlatform.Application.Features.Units.GetUnit;
using InventoryPlatform.Application.Features.Units.DeactivateUnit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Units;

public class DeactivateModel : PageModel
{
    private readonly GetUnitHandler _getHandler;
    private readonly DeactivateUnitHandler _deactivateHandler;

    public DeactivateModel(
        GetUnitHandler getHandler,
        DeactivateUnitHandler deactivateHandler)
    {
        _getHandler = getHandler;
        _deactivateHandler = deactivateHandler;
    }

    public GetUnitResponse Unit { get; private set; } = default!;

    [BindProperty]
    public DeactivateUnitRequest DeactivateRequest { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Unit = result.Value!;

        DeactivateRequest = new DeactivateUnitRequest(Unit.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _deactivateHandler.HandleAsync(DeactivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Unit '{result.Value!.Name}' has been deactivated.";

        return RedirectToPage("Index");
    }
}