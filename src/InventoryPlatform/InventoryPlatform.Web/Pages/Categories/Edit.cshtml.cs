using InventoryPlatform.Application.Features.Categories.GetCategory;
using InventoryPlatform.Application.Features.Categories.UpdateCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Categories;

public class EditModel : PageModel
{
    private readonly GetCategoryHandler _getHandler;
    private readonly UpdateCategoryHandler _updateHandler;

    public EditModel(
        GetCategoryHandler getHandler,
        UpdateCategoryHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [BindProperty]
    public UpdateCategoryRequest Category { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        var category = result.Value;

        Category = new UpdateCategoryRequest(
            category.Id,
            category.Name,
            category.Description);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _updateHandler.HandleAsync(Category);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        SuccessMessage = $"Category '{result.Value!.Name}' updated successfully.";

        return RedirectToPage("Index");
    }
}