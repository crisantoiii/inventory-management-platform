using InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroups;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Groups;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class IndexModel : PageModel
{
    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetAuthorizationGroupsHandler _handler;

    public IndexModel(GetAuthorizationGroupsHandler handler)
    {
        _handler = handler;
    }

    public IReadOnlyList<GetAuthorizationGroupsResponse> Groups { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Authorization Groups";

        Groups = await _handler.HandleAsync(cancellationToken);
    }
}
