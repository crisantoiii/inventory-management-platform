using InventoryPlatform.Application.Features.Categories.GetCategory;
using InventoryPlatform.Application.Features.Categories.DeactivateCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Categories;

public class DeactivateModel : PageModel
{
    private readonly GetCategoryHandler _getHandler;
    private readonly DeactivateCategoryHandler _deactivateHandler;

    public DeactivateModel(
        GetCategoryHandler getHandler,
        DeactivateCategoryHandler deactivateHandler)
    {
        _getHandler = getHandler;
        _deactivateHandler = deactivateHandler;
    }

    public GetCategoryResponse Category { get; private set; } = default!;

    [BindProperty]
    public DeactivateCategoryRequest DeactivateRequest { get; set; } = default!;

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _getHandler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Category = result.Value!;

        DeactivateRequest = new DeactivateCategoryRequest(Category.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _deactivateHandler.HandleAsync(DeactivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Category '{result.Value!.Name}' has been deactivated.";

        return RedirectToPage("Index");
    }
}