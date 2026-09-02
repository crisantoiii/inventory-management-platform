namespace InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroups;

public sealed record GetAuthorizationGroupsResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int CapabilityCount { get; init; }

    public int UserCount { get; init; }
}
