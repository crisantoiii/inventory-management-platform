using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Features.Reporting.GetInventoryValuation;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Reports.InventoryValuation;

public class IndexModel : PageModel
{
    private readonly GetInventoryValuationHandler _handler;

    public IndexModel(GetInventoryValuationHandler handler)
    {
        _handler = handler;
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
}