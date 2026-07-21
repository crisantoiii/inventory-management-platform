using InventoryPlatform.Application.Features.Suppliers.GetSupplier;
using InventoryPlatform.Application.Features.Suppliers.ActivateSupplier;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Suppliers;

public class ActivateModel : PageModel
{
    private readonly GetSupplierHandler _getHandler;
    private readonly ActivateSupplierHandler _activateHandler;

    public ActivateModel(
        GetSupplierHandler getHandler,
        ActivateSupplierHandler activateHandler)
    {
        _getHandler = getHandler;
        _activateHandler = activateHandler;
    }

    public GetSupplierResponse Supplier { get; private set; } = default!;

    [BindProperty]
    public ActivateSupplierRequest ActivateRequest { get; set; } = default!;

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

        ActivateRequest = new ActivateSupplierRequest(Supplier.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _activateHandler.HandleAsync(ActivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Supplier '{result.Value!.Name}' has been activated.";

        return RedirectToPage("Index");
    }
}