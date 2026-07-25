using InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.InventoryTransactions;

public class DetailsModel : PageModel
{
    private readonly GetInventoryTransactionHandler _handler;

    public DetailsModel(GetInventoryTransactionHandler handler)
    {
        _handler = handler;
    }

    public GetInventoryTransactionResponse InventoryTransaction { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        InventoryTransaction = result.Value;

        return Page();
    }
}