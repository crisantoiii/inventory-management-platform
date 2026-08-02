using InventoryPlatform.Application.Features.Products.DeactivateProduct;
using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class DeactivateModel : PageModel
{
    private readonly GetProductHandler _getHandler;
    private readonly DeactivateProductHandler _deactivateHandler;

    public DeactivateModel(
        GetProductHandler getHandler,
        DeactivateProductHandler deactivateHandler)
    {
        _getHandler = getHandler;
        _deactivateHandler = deactivateHandler;
    }

    public GetProductResponse Product { get; private set; } = default!;

    [BindProperty]
    public DeactivateProductRequest DeactivateRequest { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Deactivate Product";

        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Product = result.Value!;

        DeactivateRequest = new DeactivateProductRequest(Product.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _deactivateHandler.HandleAsync(DeactivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Product '{result.Value!.Name}' has been deactivated.";

        return RedirectToPage("Index");
    }
}