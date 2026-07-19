using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Application.Features.Products.ActivateProduct;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

public class ActivateModel : PageModel
{
    private readonly GetProductHandler _getHandler;
    private readonly ActivateProductHandler _activateHandler;

    public ActivateModel(
        GetProductHandler getHandler,
        ActivateProductHandler activateHandler)
    {
        _getHandler = getHandler;
        _activateHandler = activateHandler;
    }

    public GetProductResponse Product { get; private set; } = default!;

    [BindProperty]
    public ActivateProductRequest ActivateRequest { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Product = result.Value!;

        ActivateRequest = new ActivateProductRequest(Product.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _activateHandler.HandleAsync(ActivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Product '{result.Value!.Name}' has been activated.";

        return RedirectToPage("Index");
    }
}