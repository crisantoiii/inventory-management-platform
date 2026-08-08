using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Purchasing.PurchaseOrders;

public class IndexModel : PageModel
{
    private readonly GetPurchaseOrdersHandler _handler;

    public IndexModel(GetPurchaseOrdersHandler handler)
    {
        _handler = handler;
    }

    public IReadOnlyCollection<GetPurchaseOrderSummaryResponse> PurchaseOrders { get; private set; }
        = Array.Empty<GetPurchaseOrderSummaryResponse>();

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(
            new GetPurchaseOrdersRequest(),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        PurchaseOrders = result.Value!.PurchaseOrders;

        return Page();
    }
}