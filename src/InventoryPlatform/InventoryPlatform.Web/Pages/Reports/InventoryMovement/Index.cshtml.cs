using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetInventoryMovement;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using InventoryPlatform.Web.Reports.Excel;
using InventoryPlatform.Web.Reports.Pdf;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.InventoryMovement;

public sealed class IndexModel : PageModel
{
    private readonly GetInventoryMovementHandler _handler;
    private readonly ExcelReportWriter _excelReportWriter;
    private readonly PdfReportWriter _pdfReportWriter;

    public IndexModel(
        GetInventoryMovementHandler handler,
        ExcelReportWriter excelReportWriter,
        PdfReportWriter pdfReportWriter)
    {
        _handler = handler;
        _excelReportWriter = excelReportWriter;
        _pdfReportWriter = pdfReportWriter;
    }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; }
    [BindProperty(SupportsGet = true)] public bool Descending { get; set; }

    public PagedResult<InventoryMovementDto>? Result { get; private set; }

    public int TotalPages => Result is null || Result.PageSize <= 0
        ? 0
        : (int)Math.Ceiling(Result.TotalCount / (double)Result.PageSize);

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (PageNum < 1) PageNum = 1;
        if (PageSize < 1) PageSize = 10;

        var query = new PagedQuery
        {
            Search = Search,
            Page = PageNum,
            PageSize = PageSize,
            SortBy = SortBy,
            Descending = Descending
        };

        var request = new GetInventoryMovementRequest(query, FromDate, ToDate);
        var result = await _handler.HandleAsync(request, cancellationToken);

        if (result.IsFailure) return BadRequest();

        Result = result.Value;
        return Page();
    }
    public async Task<IActionResult> OnGetExportToExcelAsync(
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
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

        var result = await _handler.HandleExportAsync(
            new GetInventoryMovementRequest(query, fromDate, toDate),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest();

        var bytes = _excelReportWriter.CreateInventoryMovement(
            result.Value ?? Array.Empty<InventoryMovementDto>());

        return File(
            bytes,
            ExcelReportWriter.ContentType,
            "InventoryMovement.xlsx");
    }

    public async Task<IActionResult> OnGetExportToPdfAsync(
        string? search, DateOnly? fromDate, DateOnly? toDate, string? sortBy,
        bool descending = false, CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery { Search = search, Page = 1, PageSize = int.MaxValue, SortBy = sortBy, Descending = descending };
        var result = await _handler.HandleExportAsync(new GetInventoryMovementRequest(query, fromDate, toDate), cancellationToken);
        if (result.IsFailure) return BadRequest();
        var bytes = _pdfReportWriter.CreateInventoryMovement(result.Value ?? Array.Empty<InventoryMovementDto>());
        return File(bytes, PdfReportWriter.ContentType, "InventoryMovement.pdf");
    }
}
