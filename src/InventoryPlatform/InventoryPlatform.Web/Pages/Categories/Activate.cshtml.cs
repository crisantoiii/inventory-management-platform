using InventoryPlatform.Application.Features.Categories.GetCategory;
using InventoryPlatform.Application.Features.Categories.ActivateCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Categories;

public class ActivateModel : PageModel
{
    private readonly GetCategoryHandler _getHandler;
    private readonly ActivateCategoryHandler _activateHandler;

    public ActivateModel(
        GetCategoryHandler getHandler,
        ActivateCategoryHandler activateHandler)
    {
        _getHandler = getHandler;
        _activateHandler = activateHandler;
    }

    public GetCategoryResponse Category { get; private set; } = default!;

    [BindProperty]
    public ActivateCategoryRequest ActivateRequest { get; set; } = default!;

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

        ActivateRequest = new ActivateCategoryRequest(Category.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _activateHandler.HandleAsync(ActivateRequest);

        if (result.IsFailure)
        {
            return NotFound();
        }

        SuccessMessage = $"Category '{result.Value!.Name}' has been activated.";

        return RedirectToPage("Index");
    }
}