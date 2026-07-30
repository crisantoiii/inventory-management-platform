using InventoryPlatform.Application.Features.InventoryTransactions.CreateInventoryTransaction;
using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.InventoryTransactions;

[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]
public class CreateModel : PageModel
{
    private readonly CreateInventoryTransactionHandler _handler;
    private readonly GetProductsHandler _getProductsHandler;

    public CreateModel(CreateInventoryTransactionHandler handler,
        GetProductsHandler getProductsHandler
        )
    {
        _handler = handler;
        _getProductsHandler = getProductsHandler;
    }

    [BindProperty]
    public CreateInventoryTransactionRequest Transaction { get; set; } = new();
    public SelectList ProductOptions { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Create New Inventory Transaction";

        await PopulateProductDropdownAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductDropdownAsync(cancellationToken);
            return Page();
        }

        var result = await _handler.HandleAsync(Transaction);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);
            
            await PopulateProductDropdownAsync(cancellationToken);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Inventory transaction '{result.Value?.ReferenceNumber}' was created successfully.";

        return RedirectToPage("Index");
    }

    private async Task PopulateProductDropdownAsync(CancellationToken cancellationToken)
    {
        var productResult = await _getProductsHandler.HandleAsync(new GetProductsRequest(), cancellationToken);
        var products = productResult.Value?.Items
            .Select(p => new
            {
                p.Id,
                Display = $"{p.Sku} - {p.Name}"
            });

        ProductOptions = new SelectList(products, "Id", "Display");
    }
}