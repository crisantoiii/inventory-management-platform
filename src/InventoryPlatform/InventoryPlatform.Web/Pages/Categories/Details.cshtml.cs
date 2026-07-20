using InventoryPlatform.Application.Features.Categories.GetCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Categories;

public class DetailsModel : PageModel
{
    private readonly GetCategoryHandler _handler;

    public DetailsModel(GetCategoryHandler handler)
    {
        _handler = handler;
    }

    public GetCategoryResponse Category { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Category = result.Value;

        return Page();
    }
}