using InventoryPlatform.Application.Features.Customers.CreateCustomer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly CreateCustomerHandler _handler;

    public CreateModel(CreateCustomerHandler handler)
    {
        _handler = handler;
    }

    [BindProperty]
    public CreateCustomerRequest Customer { get; set; } = default!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _handler.HandleAsync(Customer);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Customer '{result.Value!.Name}' was created successfully.";

        return RedirectToPage("Index");
    }
}