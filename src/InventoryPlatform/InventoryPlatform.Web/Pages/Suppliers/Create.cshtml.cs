using InventoryPlatform.Application.Features.Suppliers.CreateSupplier;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Suppliers;

[Authorize(Policy = AuthorizationPolicies.ViewInventory)]
public class CreateModel : PageModel
{
    private readonly CreateSupplierHandler _handler;

    public CreateModel(CreateSupplierHandler handler)
    {
        _handler = handler;
    }

    [BindProperty]
    public CreateSupplierRequest Supplier { get; set; } = default!;

    public void OnGet()
    {
        ViewData["Title"] = "Create New Supplier";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _handler.HandleAsync(Supplier);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Supplier '{result.Value!.Name}' was created successfully.";

        return RedirectToPage("Index");
    }
}