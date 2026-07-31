
using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryPlatform.Web.Pages.Administrator.Users;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public class IndexModel : PageModel
{
    [FromQuery]
    public GetUsersRequest Filter { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    private readonly GetUsersHandler _handler;

    public IndexModel(GetUsersHandler handler)
    {
        _handler = handler;
    }

    public PagedResult<GetUsersResponse> Users { get; private set; } = default!;
    public IEnumerable<SelectListItem> StatusOptions =>
    [
        new()
        {
            Value = ProductStatusFilter.Active.ToString(),
            Text = "Active"
        },
        new()
        {
            Value = ProductStatusFilter.Inactive.ToString(),
            Text = "Inactive"
        },
        new()
        {
            Value = ProductStatusFilter.All.ToString(),
            Text = "All Users"
        }
    ];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Users";

        var result = await _handler.HandleAsync(Filter);

        if (result.IsSuccess && result.Value is not null)
        {
            Users = result.Value;
        }
    }
}

