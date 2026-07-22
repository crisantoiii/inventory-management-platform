using InventoryPlatform.Application.Features.Customers.GetCustomer;
using InventoryPlatform.Application.Features.Customers.DeactivateCustomer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Customers;

public class DeactivateModel : PageModel
{
    private readonly GetCustomerHandler _getHandler;
    private readonly DeactivateCustomerHandler _deactivateHandler;

    public DeactivateModel(
        GetCustomerHandler getHandler,
        DeactivateCustomerHandler deactivateHandler)
    {
        _getHandler = getHandler;
        _deactivateHandler = deactivateHandler;
    }

    public GetCustomerResponse Customer { get; private set; } = default!;

    [BindProperty]
    public DeactivateCustomerRequest DeactivateRequest { get; set; } = default!;

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

        DeactivateRequest = new DeactivateCustomerRequest(Customer.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _deactivateHandler.HandleAsync(DeactivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Customer '{result.Value!.Name}' has been deactivated.";

        return RedirectToPage("Index");
    }
}