namespace InventoryPlatform.Application.Features.Users.UpdateUserRoles;

public sealed record UpdateUserRolesRequest
{
    public Guid Id { get; init; }

    public List<string> Roles { get; init; } = [];
}