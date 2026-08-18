using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetInventoryValuation;
using InventoryPlatform.Web.Reports.Excel;
using InventoryPlatform.Web.Reports.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.InventoryValuation;

public class IndexModel : PageModel
{
    private readonly GetInventoryValuationHandler _handler;
    private readonly ExcelReportWriter _excelReportWriter;
    private readonly PdfReportWriter _pdfReportWriter;

    public IndexModel(
        GetInventoryValuationHandler handler,
        ExcelReportWriter excelReportWriter,
        PdfReportWriter pdfReportWriter)
    {
        _handler = handler;
        _excelReportWriter = excelReportWriter;
        _pdfReportWriter = pdfReportWriter;
    }

    public IReadOnlyList<InventoryValuationDto> Items { get; private set; }
        = Array.Empty<InventoryValuationDto>();

    public decimal TotalInventoryValue { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(
            new GetInventoryValuationRequest(),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return;
        }

        Items = result.Value ?? Array.Empty<InventoryValuationDto>();

        TotalInventoryValue = Items.Sum(x => x.InventoryValue);
    }
    public async Task<IActionResult> OnGetExportToExcelAsync(
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleExportAsync(
            cancellationToken);

        if (result.IsFailure)
            return BadRequest();

        var bytes = _excelReportWriter.CreateInventoryValuation(
            result.Value ?? Array.Empty<InventoryValuationDto>());

        return File(
            bytes,
            ExcelReportWriter.ContentType,
            "InventoryValuation.xlsx");
    }

    public async Task<IActionResult> OnGetExportToPdfAsync(
        CancellationToken cancellationToken)
    {
        var result = await _handler.HandleExportAsync(cancellationToken);
        if (result.IsFailure) return BadRequest();
        var bytes = _pdfReportWriter.CreateInventoryValuation(result.Value ?? Array.Empty<InventoryValuationDto>());
        return File(bytes, PdfReportWriter.ContentType, "InventoryValuation.pdf");
    }
}
