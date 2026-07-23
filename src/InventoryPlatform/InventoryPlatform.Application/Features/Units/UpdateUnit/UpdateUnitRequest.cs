namespace InventoryPlatform.Application.Features.Units.UpdateUnit;

public sealed record UpdateUnitRequest(
    int Id,
    string Code,
    string Name,
    string Symbol);