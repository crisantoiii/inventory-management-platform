using InventoryPlatform.Application.Features.Products.ActivateProduct;
using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
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
        ViewData["Title"] = "Activate Product";

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