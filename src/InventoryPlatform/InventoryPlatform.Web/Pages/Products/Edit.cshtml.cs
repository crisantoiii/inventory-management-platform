using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Application.Features.Products.UpdateProduct;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

public class EditModel : PageModel
{
    private readonly GetProductHandler _getHandler;
    private readonly UpdateProductHandler _updateHandler;

    public EditModel(
        GetProductHandler getHandler,
        UpdateProductHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateProductRequest Product { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        var product = result.Value;

        Product = new UpdateProductRequest(
            product.Id,
            product.Name,
            product.Unit,
            product.CostPrice,
            product.SellingPrice,
            product.Barcode,
            product.Description);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _updateHandler.HandleAsync(Product);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        SuccessMessage = $"Product '{result.Value!.Name}' updated successfully.";

        return RedirectToPage("Index");
    }
}