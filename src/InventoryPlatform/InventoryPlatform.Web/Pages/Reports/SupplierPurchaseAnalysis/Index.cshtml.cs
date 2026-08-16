using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetSupplierPurchaseAnalysis;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.SupplierPurchaseAnalysis;

public class IndexModel : PageModel
{
    private readonly GetSupplierPurchaseAnalysisHandler _handler;

    public IndexModel(GetSupplierPurchaseAnalysisHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<SupplierPurchaseAnalysisDto>? Result { get; private set; }

    public string? Search { get; private set; }

    public DateOnly? FromDate { get; private set; }

    public DateOnly? ToDate { get; private set; }

    public string? Status { get; private set; }

    public string? SortBy { get; private set; }

    public bool Descending { get; private set; }

    public async Task OnGetAsync(
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        string? sortBy,
        bool descending = false,
        [FromQuery(Name = "currentPage")] int currentPage = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Search = search;
        FromDate = fromDate;
        ToDate = toDate;
        Status = status;
        SortBy = sortBy;
        Descending = descending;

        PurchaseOrderStatus? purchaseOrderStatus = null;

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PurchaseOrderStatus>(
                status,
                true,
                out var parsedStatus))
        {
            purchaseOrderStatus = parsedStatus;
        }

        var request = new GetSupplierPurchaseAnalysisRequest
        {
            Search = search,
            FromDate = fromDate,
            ToDate = toDate,
            PurchaseOrderStatus = purchaseOrderStatus,
            SortBy = sortBy,
            Descending = descending,
            Page = currentPage,
            PageSize = pageSize
        };

        var result = await _handler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return;
        }

        Result = result.Value;
    }
}
