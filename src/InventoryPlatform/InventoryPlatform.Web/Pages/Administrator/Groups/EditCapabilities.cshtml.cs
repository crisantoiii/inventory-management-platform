using InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroup;
using InventoryPlatform.Application.Features.AuthorizationGroups.ManageGroupCapabilities;
using InventoryPlatform.Application.Features.Capabilities.GetCapabilities;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Groups;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class EditCapabilitiesModel : PageModel
{
    private readonly GetAuthorizationGroupHandler _getGroupHandler;
    private readonly GetCapabilitiesHandler _getCapabilitiesHandler;
    private readonly ManageGroupCapabilitiesHandler _manageHandler;

    public EditCapabilitiesModel(
        GetAuthorizationGroupHandler getGroupHandler,
        GetCapabilitiesHandler getCapabilitiesHandler,
        ManageGroupCapabilitiesHandler manageHandler)
    {
        _getGroupHandler = getGroupHandler;
        _getCapabilitiesHandler = getCapabilitiesHandler;
        _manageHandler = manageHandler;
    }

    [BindProperty]
    public int GroupId { get; set; }

    public IList<CapabilityOptionItem> CapabilityOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Manage Group Capabilities";

        GroupId = id;

        await LoadDataAsync(id, cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int id,
        List<int> selectedCapabilityIds,
        CancellationToken cancellationToken)
    {
        GroupId = id;

        var request = new ManageGroupCapabilitiesRequest
        {
            GroupId = id,
            CapabilityIds = selectedCapabilityIds ?? []
        };

        var result = await _manageHandler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            await LoadDataAsync(id, cancellationToken);
            return Page();
        }

        TempData["SuccessMessage"] =
            "Group capabilities were updated successfully.";

        return RedirectToPage("Details", new { id });
    }

    private async Task LoadDataAsync(int groupId, CancellationToken cancellationToken)
    {
        var groupResult = await _getGroupHandler.HandleAsync(
            new GetAuthorizationGroupRequest(groupId),
            cancellationToken);

        var capabilities = await _getCapabilitiesHandler.HandleAsync(cancellationToken);

        var currentCapIds = groupResult.IsSuccess && groupResult.Value is not null
            ? groupResult.Value.Capabilities.Select(c => c.Id).ToHashSet()
            : new HashSet<int>();

        CapabilityOptions = capabilities
            .Select(c => new CapabilityOptionItem
            {
                Id = c.Id,
                Name = c.Name,
                Selected = currentCapIds.Contains(c.Id)
            })
            .ToList();
    }

    public class CapabilityOptionItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Selected { get; set; }
    }
}
