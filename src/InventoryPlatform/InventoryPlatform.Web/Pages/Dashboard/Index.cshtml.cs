using InventoryPlatform.Application.DTOs.Dashboard;
using InventoryPlatform.Application.Features.Customers.GetCustomers;
using InventoryPlatform.Application.Features.Dashboard.GetDashboard;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.WebRequestMethods;

namespace InventoryPlatform.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    [FromQuery]
    public GetDashboardQuery Query { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetDashboardHandler _handler;

    public IndexModel(GetDashboardHandler handler)
    {
        _handler = handler;
    }

    public DashboardDto DashbordDTO { get; private set; } = default!;

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Dashboard";

        var result = await _handler.HandleAsync(Query, cancellationToken);

        if (result.IsSuccess && result.Value is not null)
        {
            DashbordDTO = result.Value;
        }
    }
}
