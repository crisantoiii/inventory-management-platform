using InventoryPlatform.Application.Features.Suppliers.GetSupplier;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Suppliers;

public class DetailsModel : PageModel
{
    private readonly GetSupplierHandler _handler;

    public DetailsModel(GetSupplierHandler handler)
    {
        _handler = handler;
    }

    public GetSupplierResponse Supplier { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Supplier = result.Value;

        return Page();
    }
}