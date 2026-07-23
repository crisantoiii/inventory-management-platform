namespace InventoryPlatform.Application.Features.Units.ActivateUnit;

public sealed record ActivateUnitResponse(
    int Id,
    string Name,
    bool IsActive);