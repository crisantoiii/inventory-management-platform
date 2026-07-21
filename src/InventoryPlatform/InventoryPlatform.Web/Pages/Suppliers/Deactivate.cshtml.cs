using InventoryPlatform.Application.Features.Suppliers.GetSupplier;
using InventoryPlatform.Application.Features.Suppliers.DeactivateSupplier;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Suppliers;

public class DeactivateModel : PageModel
{
    private readonly GetSupplierHandler _getHandler;
    private readonly DeactivateSupplierHandler _deactivateHandler;

    public DeactivateModel(
        GetSupplierHandler getHandler,
        DeactivateSupplierHandler deactivateHandler)
    {
        _getHandler = getHandler;
        _deactivateHandler = deactivateHandler;
    }

    public GetSupplierResponse Supplier { get; private set; } = default!;

    [BindProperty]
    public DeactivateSupplierRequest DeactivateRequest { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Supplier = result.Value!;

        DeactivateRequest = new DeactivateSupplierRequest(Supplier.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _deactivateHandler.HandleAsync(DeactivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Supplier '{result.Value!.Name}' has been deactivated.";

        return RedirectToPage("Index");
    }
}