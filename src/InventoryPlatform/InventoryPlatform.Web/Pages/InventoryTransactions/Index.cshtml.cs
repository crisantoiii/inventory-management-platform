using InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransactions;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Filtering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.InventoryTransactions;

public class IndexModel : PageModel
{
    [FromQuery]
    public GetInventoryTransactionsRequest Filter { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetInventoryTransactionsHandler _handler;

    public IndexModel(GetInventoryTransactionsHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<GetInventoryTransactionsResponse> InventoryTransactions { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Inventory Transactions";

        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            InventoryTransactions = result.Value;
        }
    }
}