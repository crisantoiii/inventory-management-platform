namespace InventoryPlatform.Application.Features.AuthorizationGroups.CreateAuthorizationGroup;

public sealed record CreateAuthorizationGroupRequest
{
    public string Name { get; init; } = string.Empty;
}
