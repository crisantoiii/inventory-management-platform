using InventoryPlatform.Application.Features.Suppliers.GetSupplier;
using InventoryPlatform.Application.Features.Suppliers.UpdateSupplier;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Suppliers;

public class EditModel : PageModel
{
    private readonly GetSupplierHandler _getHandler;
    private readonly UpdateSupplierHandler _updateHandler;

    public EditModel(
        GetSupplierHandler getHandler,
        UpdateSupplierHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateSupplierRequest Supplier { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        var supplier = result.Value;

        Supplier = new UpdateSupplierRequest(
            supplier.Id,
            supplier.Name,
            supplier.ContactPerson,
            supplier.Email,
            supplier.Phone,
            supplier.Address);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _updateHandler.HandleAsync(Supplier);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        SuccessMessage = $"Supplier '{result.Value!.Name}' updated successfully.";

        return RedirectToPage("Index");
    }
}