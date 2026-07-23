namespace InventoryPlatform.Application.Features.Units.DeactivateUnit;

public sealed record DeactivateUnitResponse(
    int Id,
    string Name,
    bool IsActive);