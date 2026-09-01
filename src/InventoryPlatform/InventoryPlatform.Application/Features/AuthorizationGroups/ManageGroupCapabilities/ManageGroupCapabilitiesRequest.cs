namespace InventoryPlatform.Application.Features.AuthorizationGroups.ManageGroupCapabilities;

public sealed record ManageGroupCapabilitiesRequest
{
    public int GroupId { get; init; }

    public IReadOnlyList<int> CapabilityIds { get; init; } = [];
}
