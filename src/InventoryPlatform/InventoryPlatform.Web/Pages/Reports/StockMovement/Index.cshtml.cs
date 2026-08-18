using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetStockMovement;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using InventoryPlatform.Web.Reports.Excel;
using InventoryPlatform.Web.Reports.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.StockMovement;

public sealed class IndexModel : PageModel
{
    private readonly GetStockMovementHandler _handler;
    private readonly ExcelReportWriter _excelReportWriter;
    private readonly PdfReportWriter _pdfReportWriter;

    public IndexModel(
        GetStockMovementHandler handler,
        ExcelReportWriter excelReportWriter,
        PdfReportWriter pdfReportWriter)
    {
        _handler = handler;
        _excelReportWriter = excelReportWriter;
        _pdfReportWriter = pdfReportWriter;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public TransactionType? TransactionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Descending { get; set; }

    public PagedResult<StockMovementDto>? Result { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        if (Page < 1)
            Page = 1;

        if (PageSize < 1)
            PageSize = 10;

        var query = new PagedQuery
        {
            Search = Search,
            Page = Page,
            PageSize = PageSize,
            SortBy = SortBy,
            Descending = Descending
        };

        var request = new GetStockMovementRequest(
            query,
            FromDate,
            ToDate,
            TransactionType);

        var result = await _handler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest();

        Result = result.Value;

        return Page();
    }

    public async Task<IActionResult> OnGetExportToExcelAsync(
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        TransactionType? transactionType,
        string? sortBy,
        bool descending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Search = search,
            Page = 1,
            PageSize = int.MaxValue,
            SortBy = sortBy,
            Descending = descending
        };

        var request = new GetStockMovementRequest(
            query,
            fromDate,
            toDate,
            transactionType);

        var result = await _handler.HandleExportAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest();

        var bytes = _excelReportWriter.CreateStockMovement(
            result.Value ?? Array.Empty<StockMovementDto>());

        return File(
            bytes,
            ExcelReportWriter.ContentType,
            "StockMovement.xlsx");
    }

    public async Task<IActionResult> OnGetExportToPdfAsync(
        string? search, DateOnly? fromDate, DateOnly? toDate, TransactionType? transactionType,
        string? sortBy, bool descending = false, CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery { Search = search, Page = 1, PageSize = int.MaxValue, SortBy = sortBy, Descending = descending };
        var request = new GetStockMovementRequest(query, fromDate, toDate, transactionType);
        var result = await _handler.HandleExportAsync(request, cancellationToken);
        if (result.IsFailure) return BadRequest();
        var bytes = _pdfReportWriter.CreateStockMovement(result.Value ?? Array.Empty<StockMovementDto>());
        return File(bytes, PdfReportWriter.ContentType, "StockMovement.pdf");
    }
}
