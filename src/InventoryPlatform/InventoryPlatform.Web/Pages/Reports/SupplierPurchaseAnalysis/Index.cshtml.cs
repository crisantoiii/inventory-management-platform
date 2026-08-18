using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetSupplierPurchaseAnalysis;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Web.Reports.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.SupplierPurchaseAnalysis;

public class IndexModel : PageModel
{
    private readonly GetSupplierPurchaseAnalysisHandler _handler;
    private readonly ExcelReportWriter _excelReportWriter;

    public IndexModel(
        GetSupplierPurchaseAnalysisHandler handler,
        ExcelReportWriter excelReportWriter)
    {
        _handler = handler;
        _excelReportWriter = excelReportWriter;
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

    public async Task<IActionResult> OnGetExportToExcelAsync(
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        string? sortBy,
        bool descending = false,
        CancellationToken cancellationToken = default)
    {
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
            Descending = descending
        };

        var result = await _handler.HandleExportAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest();

        var bytes = _excelReportWriter.CreateSupplierPurchaseAnalysis(
            result.Value ?? Array.Empty<SupplierPurchaseAnalysisDto>());

        return File(
            bytes,
            ExcelReportWriter.ContentType,
            "SupplierPurchaseAnalysis.xlsx");
    }
}
