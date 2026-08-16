using InventoryPlatform.Application.Features.Reporting.GetStockMovement;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.StockMovement;

public sealed class IndexModel : PageModel
{
    private readonly GetStockMovementHandler _handler;

    public IndexModel(
        GetStockMovementHandler handler)
    {
        _handler = handler;
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

    public PagedResult<InventoryPlatform.Application.DTOs.Reporting.StockMovementDto>? Result { get; private set; }

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
}