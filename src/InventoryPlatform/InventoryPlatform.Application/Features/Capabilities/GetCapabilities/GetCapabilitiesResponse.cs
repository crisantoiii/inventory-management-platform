namespace InventoryPlatform.Application.Features.Capabilities.GetCapabilities;

public sealed record GetCapabilitiesResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}
