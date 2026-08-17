using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetProductReports;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Reports.ProductReports;

public sealed class IndexModel : PageModel
{
    private readonly GetProductReportsHandler _handler;

    public IndexModel(
        GetProductReportsHandler handler)
    {
        _handler = handler;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public ProductStatusFilter Status { get; set; } = ProductStatusFilter.Active;

    [BindProperty(SupportsGet = true)]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Descending { get; set; }

    public IEnumerable<SelectListItem> StatusOptions =>
    [
        new()
        {
            Value = ProductStatusFilter.Active.ToString(),
            Text = "Active"
        },
        new()
        {
            Value = ProductStatusFilter.Inactive.ToString(),
            Text = "Inactive"
        },
        new()
        {
            Value = ProductStatusFilter.All.ToString(),
            Text = "All Products"
        }
    ];

    public PagedResult<ProductReportDto>? Result { get; private set; }

    public int TotalPages =>
        Result is null || Result.PageSize <= 0
            ? 0
            : (int)Math.Ceiling(
                Result.TotalCount / (double)Result.PageSize);

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        if (PageNum < 1)
        {
            PageNum = 1;
        }

        if (PageSize < 1)
        {
            PageSize = 10;
        }

        var query = new PagedQuery
        {
            Search = Search,
            Status = Status,
            Page = PageNum,
            PageSize = PageSize,
            SortBy = SortBy,
            Descending = Descending
        };

        var result = await _handler.HandleAsync(
            new GetProductReportsRequest(query),
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest();
        }

        Result = result.Value;

        return Page();
    }
}
