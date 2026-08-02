using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Users;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class DetailsModel : PageModel
{
    private readonly GetUserHandler _handler;

    public DetailsModel(GetUserHandler handler)
    {
        _handler = handler;
    }

    public GetUserResponse User { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        ViewData["Title"] = "User Details";

        var result = await _handler.HandleAsync(id);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        User = result.Value;

        return Page();
    }
}