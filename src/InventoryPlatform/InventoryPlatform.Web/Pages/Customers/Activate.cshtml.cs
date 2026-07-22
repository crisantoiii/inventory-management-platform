using InventoryPlatform.Application.Features.Customers.GetCustomer;
using InventoryPlatform.Application.Features.Customers.ActivateCustomer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Customers;

public class ActivateModel : PageModel
{
    private readonly GetCustomerHandler _getHandler;
    private readonly ActivateCustomerHandler _activateHandler;

    public ActivateModel(
        GetCustomerHandler getHandler,
        ActivateCustomerHandler activateHandler)
    {
        _getHandler = getHandler;
        _activateHandler = activateHandler;
    }

    public GetCustomerResponse Customer { get; private set; } = default!;

    [BindProperty]
    public ActivateCustomerRequest ActivateRequest { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Customer = result.Value!;

        ActivateRequest = new ActivateCustomerRequest(Customer.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _activateHandler.HandleAsync(ActivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Customer '{result.Value!.Name}' has been activated.";

        return RedirectToPage("Index");
    }
}