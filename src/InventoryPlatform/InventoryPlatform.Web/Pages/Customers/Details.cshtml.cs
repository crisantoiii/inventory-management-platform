using InventoryPlatform.Application.Features.Customers.GetCustomer;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Customers;

[Authorize(Policy = AuthorizationPolicies.ViewInventory)]
public class DetailsModel : PageModel
{
    private readonly GetCustomerHandler _handler;

    public DetailsModel(GetCustomerHandler handler)
    {
        _handler = handler;
    }

    public GetCustomerResponse Customer { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["Title"] = "Customer Details";

        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Customer = result.Value;

        return Page();
    }
}