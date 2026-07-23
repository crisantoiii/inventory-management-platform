namespace InventoryPlatform.Application.Features.Units.GetUnit;

public sealed record GetUnitResponse(
    int Id,
    string Code,
    string Name,
    string Symbol,
    bool IsActive);