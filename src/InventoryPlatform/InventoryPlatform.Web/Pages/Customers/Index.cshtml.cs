using InventoryPlatform.Application.Features.Customers.GetCustomers;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Filtering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Customers;

public class IndexModel : PageModel
{
    [FromQuery]
    public GetCustomersRequest Filter { get; set; } = new();

    [FromQuery]
    public ProductStatusFilter Status { get; set; }
    = ProductStatusFilter.Active;

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetCustomersHandler _handler;

    public IndexModel(GetCustomersHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<GetCustomersResponse> Customers { get; private set; } = default!;
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
            Text = "All Customers"
        }
    ];

    public async Task OnGetAsync()
    {
        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            Customers = result.Value;
        }
    }
}