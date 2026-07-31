using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Users.GetUsers;

public sealed record GetUsersRequest : PagedRequest
{
    public string? Role { get; init; }

    public bool? IsActive { get; init; }
}