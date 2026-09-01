namespace InventoryPlatform.Application.Features.AuthorizationGroups.ManageGroupUsers;

public sealed record ManageGroupUsersRequest
{
    public int GroupId { get; init; }

    public IReadOnlyList<Guid> UserIds { get; init; } = [];
}
