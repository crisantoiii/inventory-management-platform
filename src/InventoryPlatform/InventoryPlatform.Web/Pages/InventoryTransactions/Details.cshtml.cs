using InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransaction;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.InventoryTransactions;

[Authorize(Policy = AuthorizationPolicies.ViewInventory)]
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
        ViewData["Title"] = "Inventory Transaction Details";

        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        InventoryTransaction = result.Value;

        return Page();
    }
}