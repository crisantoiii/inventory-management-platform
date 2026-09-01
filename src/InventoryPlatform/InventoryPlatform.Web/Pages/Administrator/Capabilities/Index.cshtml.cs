using InventoryPlatform.Application.Features.Capabilities.GetCapabilities;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Capabilities;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class IndexModel : PageModel
{
    private readonly GetCapabilitiesHandler _handler;

    public IndexModel(GetCapabilitiesHandler handler)
    {
        _handler = handler;
    }

    public IReadOnlyList<GetCapabilitiesResponse> Capabilities { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Capability Catalog";

        Capabilities = await _handler.HandleAsync(cancellationToken);
    }
}
