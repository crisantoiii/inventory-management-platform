using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetPurchaseHistory;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.PurchaseHistory;

public class IndexModel : PageModel
{
    private readonly GetPurchaseHistoryHandler _handler;

    public IndexModel(GetPurchaseHistoryHandler handler)
    {
        _handler = handler;
    }

    public IReadOnlyList<PurchaseHistoryDto> Items { get; private set; }
        = Array.Empty<PurchaseHistoryDto>();

    public decimal TotalPurchaseAmount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(
            new GetPurchaseHistoryRequest(),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return;
        }

        Items = result.Value ?? Array.Empty<PurchaseHistoryDto>();

        TotalPurchaseAmount = Items.Sum(x => x.TotalAmount);
    }
}