using InventoryPlatform.Application.Features.Categories.GetCategories;
using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Application.Features.Products.UpdateProduct;
using InventoryPlatform.Application.Features.Units.GetUnits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Products;

public class EditModel : PageModel
{
    private readonly GetProductHandler _getHandler;
    private readonly UpdateProductHandler _updateHandler;
    private readonly GetCategoriesHandler _getCategoriesHandler;
    private readonly GetUnitsHandler _getUnitsHandler;

    public EditModel(
        GetProductHandler getHandler,
        UpdateProductHandler updateHandler,
        GetCategoriesHandler getCategoryHandler,
        GetUnitsHandler getUnitHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
        _getCategoriesHandler = getCategoryHandler;
        _getUnitsHandler = getUnitHandler;
    }

    [BindProperty]
    public UpdateProductRequest Product { get; set; } = default!;
    public SelectList CategoryOptions { get; private set; } = default!;
    public SelectList UnitOptions { get; private set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        await PopulateDropdownListsAsync(cancellationToken);

        var product = result.Value;

        Product = new UpdateProductRequest(
            product.Id,
            product.Name,
            product.CategoryId,
            product.UnitId,
            product.QuantityOnHand,
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

    private async Task PopulateDropdownListsAsync(CancellationToken cancellationToken)
    {
        var categoriesResult = await _getCategoriesHandler.HandleAsync(new GetCategoriesRequest(), cancellationToken);
        var unitsResult = await _getUnitsHandler.HandleAsync(new GetUnitsRequest(), cancellationToken);

        CategoryOptions = new SelectList(categoriesResult.Value?.Items, "Id", "Name");
        UnitOptions = new SelectList(unitsResult.Value?.Items, "Id", "Name");
    }
}