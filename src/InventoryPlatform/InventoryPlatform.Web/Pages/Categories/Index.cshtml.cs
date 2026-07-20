using InventoryPlatform.Application.Features.Categories.GetCategories;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Filtering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Categories;

public class IndexModel : PageModel
{
    [FromQuery]
    public GetCategoriesRequest Filter { get; set; } = new();

    [FromQuery]
    public ProductStatusFilter Status { get; set; }
    = ProductStatusFilter.Active;

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetCategoriesHandler _handler;

    public IndexModel(GetCategoriesHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<GetCategoriesResponse> Categories { get; private set; } = default!;
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
            Text = "All Categories"
        }
    ];

    public async Task OnGetAsync()
    {
        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            Categories = result.Value;
        }
    }
}