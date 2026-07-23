using InventoryPlatform.Application.Features.Units.GetUnit;
using InventoryPlatform.Application.Features.Units.ActivateUnit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Units;

public class ActivateModel : PageModel
{
    private readonly GetUnitHandler _getHandler;
    private readonly ActivateUnitHandler _activateHandler;

    public ActivateModel(
        GetUnitHandler getHandler,
        ActivateUnitHandler activateHandler)
    {
        _getHandler = getHandler;
        _activateHandler = activateHandler;
    }

    public GetUnitResponse Unit { get; private set; } = default!;

    [BindProperty]
    public ActivateUnitRequest ActivateRequest { get; set; } = default!;

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

        ActivateRequest = new ActivateUnitRequest(Unit.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _activateHandler.HandleAsync(ActivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Unit '{result.Value!.Name}' has been activated.";

        return RedirectToPage("Index");
    }
}