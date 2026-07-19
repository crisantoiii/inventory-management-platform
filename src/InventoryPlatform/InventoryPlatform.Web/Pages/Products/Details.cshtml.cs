using InventoryPlatform.Application.Features.Products.GetProduct;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

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
        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Product = result.Value;

        return Page();
    }
}