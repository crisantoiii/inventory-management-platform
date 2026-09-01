using InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroup;
using InventoryPlatform.Application.Features.AuthorizationGroups.ManageGroupUsers;
using InventoryPlatform.Application.Features.Users.GetAllUsers;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Administrator.Groups;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class EditUsersModel : PageModel
{
    private readonly GetAuthorizationGroupHandler _getGroupHandler;
    private readonly GetAllUsersHandler _getAllUsersHandler;
    private readonly ManageGroupUsersHandler _manageHandler;

    public EditUsersModel(
        GetAuthorizationGroupHandler getGroupHandler,
        GetAllUsersHandler getAllUsersHandler,
        ManageGroupUsersHandler manageHandler)
    {
        _getGroupHandler = getGroupHandler;
        _getAllUsersHandler = getAllUsersHandler;
        _manageHandler = manageHandler;
    }

    [BindProperty]
    public int GroupId { get; set; }

    public IList<UserOptionItem> UserOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Manage Group Users";

        GroupId = id;

        await LoadDataAsync(id, cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int id,
        List<Guid> selectedUserIds,
        CancellationToken cancellationToken)
    {
        GroupId = id;

        var request = new ManageGroupUsersRequest
        {
            GroupId = id,
            UserIds = selectedUserIds ?? []
        };

        var result = await _manageHandler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            await LoadDataAsync(id, cancellationToken);
            return Page();
        }

        TempData["SuccessMessage"] =
            "Group users were updated successfully.";

        return RedirectToPage("Details", new { id });
    }

    private async Task LoadDataAsync(int groupId, CancellationToken cancellationToken)
    {
        var groupResult = await _getGroupHandler.HandleAsync(
            new GetAuthorizationGroupRequest(groupId),
            cancellationToken);

        var users = await _getAllUsersHandler.HandleAsync(cancellationToken);

        var currentUserIds = groupResult.IsSuccess && groupResult.Value is not null
            ? groupResult.Value.Users.Select(u => u.Id).ToHashSet()
            : new HashSet<Guid>();

        UserOptions = users
            .Select(u => new UserOptionItem
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Selected = currentUserIds.Contains(u.Id)
            })
            .ToList();
    }

    public class UserOptionItem
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool Selected { get; set; }
    }
}
