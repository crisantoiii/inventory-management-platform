using InventoryPlatform.Application.Features.Units.GetUnit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Units;

public class DetailsModel : PageModel
{
    private readonly GetUnitHandler _handler;

    public DetailsModel(GetUnitHandler handler)
    {
        _handler = handler;
    }

    public GetUnitResponse Unit { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Unit = result.Value;

        return Page();
    }
}