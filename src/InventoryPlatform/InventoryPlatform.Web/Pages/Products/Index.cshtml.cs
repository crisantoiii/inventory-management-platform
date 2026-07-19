using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Filtering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Products;

public class IndexModel : PageModel
{
    [FromQuery]
    public GetProductsRequest Filter { get; set; } = new();

    [FromQuery]
    public ProductStatusFilter Status { get; set; }
    = ProductStatusFilter.Active;

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetProductsHandler _handler;

    public IndexModel(GetProductsHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<GetProductsResponse> Products { get; private set; } = default!;
    public IEnumerable<SelectListItem> StatusOptions =>
    [
        new()
        {
            Value = ProductStatusFilter.Active.ToString(),
            Text = "Active"
        },
        new()
        {
            Value = ProductStatusFilter.Inactive.ToString(),
            Text = "Inactive"
        },
        new()
        {
            Value = ProductStatusFilter.All.ToString(),
            Text = "All Products"
        }
    ];

    public async Task OnGetAsync()
    {
        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            Products = result.Value;
        }
    }
}