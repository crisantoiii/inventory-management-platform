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

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public PurchaseOrderStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Descending { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public string GetSortUrl(string sortBy)
    {
        var descending = string.Equals(SortBy, sortBy, StringComparison.OrdinalIgnoreCase)
            ? !Descending
            : false;

        return Url.Page(
            "./Index",
            values: new
            {
                Search,
                FromDate,
                ToDate,
                Status,
                SortBy = sortBy,
                Descending = descending,
                Page = 1,
                PageSize
            }) ?? string.Empty;
    }

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

    public string GetPageUrl(int page)
    {
        return Url.Page(
            "./Index",
            values: new
            {
                Search,
                FromDate,
                ToDate,
                Status,
                SortBy,
                Descending,
                Page = page,
                PageSize
            }) ?? string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(
            new GetPurchaseOrdersRequest
            {
                Search = Search,
                FromDate = FromDate,
                ToDate = ToDate,
                PurchaseOrderStatus = Status,
                SortBy = SortBy,
                Descending = Descending,
                Page = PageNum,
                PageSize = PageSize
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
