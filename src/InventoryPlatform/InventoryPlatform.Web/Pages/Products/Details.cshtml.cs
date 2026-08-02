using InventoryPlatform.Application.Features.Products.GetProduct;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

[Authorize(Policy = AuthorizationPolicies.ViewInventory)]
public class DetailsModel : PageModel
{
    private readonly GetProductHandler _handler;

    public DetailsModel(GetProductHandler handler)
    {
        _handler = handler;
    }

    public GetProductResponse Product { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Product Details";

        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Product = result.Value;

        return Page();
    }
}