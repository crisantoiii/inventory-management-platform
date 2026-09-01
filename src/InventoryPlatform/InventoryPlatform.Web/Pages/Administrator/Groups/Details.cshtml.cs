using InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroup;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Groups;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class DetailsModel : PageModel
{
    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetAuthorizationGroupHandler _handler;

    public DetailsModel(GetAuthorizationGroupHandler handler)
    {
        _handler = handler;
    }

    public GetAuthorizationGroupResponse Group { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Authorization Group Details";

        var result = await _handler.HandleAsync(
            new GetAuthorizationGroupRequest(id),
            cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        Group = result.Value;

        return Page();
    }
}
