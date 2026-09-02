using InventoryPlatform.Application.Features.AuthorizationGroups.CreateAuthorizationGroup;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Groups;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class CreateModel : PageModel
{
    private readonly CreateAuthorizationGroupHandler _handler;

    public CreateModel(CreateAuthorizationGroupHandler handler)
    {
        _handler = handler;
    }

    [BindProperty]
    public CreateAuthorizationGroupRequest Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["Title"] = "Create Authorization Group";

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _handler.HandleAsync(Input, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return Page();
        }

        TempData["SuccessMessage"] =
            $"Authorization group '{result.Value!.Name}' was created successfully.";

        return RedirectToPage("Index");
    }
}
