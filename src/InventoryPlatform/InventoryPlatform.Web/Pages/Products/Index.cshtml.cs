using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Products;

[Authorize(Policy = AuthorizationPolicies.ViewInventory)]
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
        ViewData["Title"] = "Products";

        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            Products = result.Value;
        }
    }
}