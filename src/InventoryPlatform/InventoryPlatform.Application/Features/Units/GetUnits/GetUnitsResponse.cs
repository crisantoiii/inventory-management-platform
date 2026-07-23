namespace InventoryPlatform.Application.Features.Units.GetUnits;

public sealed record GetUnitsResponse
{
    public int Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Symbol { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}