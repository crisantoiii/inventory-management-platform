namespace InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroup;

public sealed record GetAuthorizationGroupResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<CapabilitySummary> Capabilities { get; init; } = [];

    public IReadOnlyList<UserSummary> Users { get; init; } = [];
}

public sealed record CapabilitySummary
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}

public sealed record UserSummary
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}
