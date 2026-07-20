using InventoryPlatform.Application.Features.Categories.CreateCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Categories;

public class CreateModel : PageModel
{
    private readonly CreateCategoryHandler _handler;

    public CreateModel(CreateCategoryHandler handler)
    {
        _handler = handler;
    }

    [BindProperty]
    public CreateCategoryRequest Category { get; set; } = default!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _handler.HandleAsync(Category);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Category '{result.Value!.Name}' was created successfully.";

        return RedirectToPage("Index");
    }
}