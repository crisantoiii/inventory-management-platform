namespace InventoryPlatform.Application.Features.Units.CreateUnit;

public sealed record CreateUnitRequest(
    string Code,
    string Name,
    string Symbol);