using InventoryPlatform.Application.Features.Categories.GetCategories;
using InventoryPlatform.Application.Features.Products.CreateProduct;
using InventoryPlatform.Application.Features.Units.GetUnits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Products;

public class CreateModel : PageModel
{
    private readonly CreateProductHandler _handler;
    private readonly GetCategoriesHandler _getCategoriesHandler;
    private readonly GetUnitsHandler _getUnitsHandler;

    public CreateModel(CreateProductHandler handler,
        GetCategoriesHandler getCategoryHandler,
        GetUnitsHandler getUnitHandler)
    {
        _handler = handler;
        _getCategoriesHandler = getCategoryHandler;
        _getUnitsHandler = getUnitHandler;
    }

    [BindProperty]
    public CreateProductRequest Product { get; set; } = default!;
    public SelectList CategoryOptions { get; private set; } = default!;
    public SelectList UnitOptions { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await PopulateDropdownListsAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _handler.HandleAsync(Product);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);
            
            return Page();
        }

        TempData["SuccessMessage"] =
            $"Product '{result.Value!.Name}' was created successfully.";

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