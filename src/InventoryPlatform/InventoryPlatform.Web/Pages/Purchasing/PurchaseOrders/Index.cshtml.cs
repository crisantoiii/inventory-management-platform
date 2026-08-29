using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
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

    public PagedResult<GetPurchaseOrderSummaryResponse> PurchaseOrders { get; private set; }
        = new()
        {
            Items = Array.Empty<GetPurchaseOrderSummaryResponse>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

    [BindProperty(SupportsGet = true, Name = "")]
    public GetPurchaseOrdersRequest Request { get; set; } = new();

    public static class SortFields
    {
        public const string Id = PurchaseOrderSortFields.Id;
        public const string Supplier = PurchaseOrderSortFields.Supplier;
        public const string OrderDate = PurchaseOrderSortFields.OrderDate;
        public const string Status = PurchaseOrderSortFields.Status;
        public const string TotalAmount = PurchaseOrderSortFields.TotalAmount;
    }

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
        [FromQuery(Name = "Status")] PurchaseOrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        Request = Request with
        {
            PurchaseOrderStatus = status
        };

        var result = await _handler.HandleAsync(
            Request,
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
