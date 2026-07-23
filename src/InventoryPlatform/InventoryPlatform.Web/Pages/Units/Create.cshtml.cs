using InventoryPlatform.Application.Features.Units.CreateUnit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Units;

public class CreateModel : PageModel
{
    private readonly CreateUnitHandler _handler;

    public CreateModel(CreateUnitHandler handler)
    {
        _handler = handler;
    }

    [BindProperty]
    public CreateUnitRequest Unit { get; set; } = default!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _handler.HandleAsync(Unit);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            return Page();
        }

        TempData["SuccessMessage"] =
            $"Unit '{result.Value!.Name}' was created successfully.";

        return RedirectToPage("Index");
    }
}