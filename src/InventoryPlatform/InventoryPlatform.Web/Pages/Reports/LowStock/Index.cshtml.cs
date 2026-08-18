using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetLowStock;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using InventoryPlatform.Web.Reports.Excel;
using InventoryPlatform.Web.Reports.Pdf;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.LowStock;

public sealed class IndexModel : PageModel
{
    private readonly GetLowStockHandler _handler;
    private readonly ExcelReportWriter _excelReportWriter;
    private readonly PdfReportWriter _pdfReportWriter;

    public IndexModel(
        GetLowStockHandler handler,
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
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Descending { get; set; }

    public PagedResult<LowStockDto>? Result { get; private set; }

    public int TotalPages =>
        Result is null || Result.PageSize <= 0
            ? 0
            : (int)Math.Ceiling(
                Result.TotalCount / (double)Result.PageSize);

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var query = new PagedQuery
        {
            Search = Search,
            Page = PageNum,
            PageSize = PageSize,
            SortBy = SortBy,
            Descending = Descending
        };

        var request = new GetLowStockRequest(query);

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
            new GetLowStockRequest(query),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest();

        var bytes = _excelReportWriter.CreateLowStock(
            result.Value ?? Array.Empty<LowStockDto>());

        return File(
            bytes,
            ExcelReportWriter.ContentType,
            "LowStock.xlsx");
    }

    public async Task<IActionResult> OnGetExportToPdfAsync(
        string? search, string? sortBy, bool descending = false, CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery { Search = search, Page = 1, PageSize = int.MaxValue, SortBy = sortBy, Descending = descending };
        var result = await _handler.HandleExportAsync(new GetLowStockRequest(query), cancellationToken);
        if (result.IsFailure) return BadRequest();
        var bytes = _pdfReportWriter.CreateLowStock(result.Value ?? Array.Empty<LowStockDto>());
        return File(bytes, PdfReportWriter.ContentType, "LowStock.pdf");
    }
}
