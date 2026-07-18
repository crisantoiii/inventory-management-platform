using InventoryPlatform.Application.Features.Products.GetProducts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly GetProductsHandler _handler;

    public IndexModel(GetProductsHandler handler)
    {
        _handler = handler;
    }

    public IReadOnlyList<GetProductsResponse> Products { get; private set; }
        = [];

    public async Task OnGetAsync()
    {
        var result = await _handler.HandleAsync();

        if (result.IsSuccess && result.Value is not null)
        {
            Products = result.Value;
        }
    }
}