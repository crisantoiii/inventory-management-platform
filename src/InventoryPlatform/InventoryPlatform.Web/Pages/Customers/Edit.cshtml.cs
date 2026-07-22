using InventoryPlatform.Application.Features.Customers.GetCustomer;
using InventoryPlatform.Application.Features.Customers.UpdateCustomer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Customers;

public class EditModel : PageModel
{
    private readonly GetCustomerHandler _getHandler;
    private readonly UpdateCustomerHandler _updateHandler;

    public EditModel(
        GetCustomerHandler getHandler,
        UpdateCustomerHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateCustomerRequest Customer { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        var customer = result.Value;

        Customer = new UpdateCustomerRequest(
            customer.Id,
            customer.Name,
            customer.ContactPerson,
            customer.Email,
            customer.Phone,
            customer.Address);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _updateHandler.HandleAsync(Customer);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        SuccessMessage = $"Customer '{result.Value!.Name}' updated successfully.";

        return RedirectToPage("Index");
    }
}