namespace InventoryPlatform.Application.Features.AuthorizationGroups.UpdateAuthorizationGroup;

public sealed record UpdateAuthorizationGroupRequest
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
