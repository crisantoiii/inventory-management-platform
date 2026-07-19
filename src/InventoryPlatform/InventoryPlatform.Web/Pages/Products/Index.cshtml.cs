using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

public class IndexModel : PageModel
{
    [FromQuery]
    public GetProductsRequest Filter { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetProductsHandler _handler;

    public IndexModel(GetProductsHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<GetProductsResponse> Products { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            Products = result.Value;
        }
    }
}