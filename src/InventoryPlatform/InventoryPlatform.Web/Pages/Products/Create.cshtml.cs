using InventoryPlatform.Application.Features.Products.CreateProduct;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Products;

public class CreateModel : PageModel
{
    private readonly CreateProductHandler _handler;

    public CreateModel(CreateProductHandler handler)
    {
        _handler = handler;
    }

    [BindProperty]
    public CreateProductRequest Product { get; set; } = default!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _handler.HandleAsync(Product);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Product '{result.Value!.Name}' was created successfully.";

        return RedirectToPage("Index");
    }
}