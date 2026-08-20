using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;
using InventoryPlatform.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public PurchaseOrderStatus? Status { get; set; }

    public IReadOnlyList<SelectListItem> StatusOptions =>
        new List<SelectListItem>
        {
            new("All statuses", ""),
            new("Draft", nameof(PurchaseOrderStatus.Draft)),
            new("Submitted", nameof(PurchaseOrderStatus.Submitted)),
            new("Approved", nameof(PurchaseOrderStatus.Approved)),
            new("Receiving", nameof(PurchaseOrderStatus.Receiving)),
            new("Completed", nameof(PurchaseOrderStatus.Completed)),
            new("Cancelled", nameof(PurchaseOrderStatus.Cancelled))
        };

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(
            new GetPurchaseOrdersRequest
            {
                Search = Search,
                FromDate = FromDate,
                ToDate = ToDate,
                Status = Status
            },
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
