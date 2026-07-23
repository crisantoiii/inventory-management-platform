using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Units;

public static class UnitErrors
{
    public static readonly Error NotFound =
        new(
            "Unit.NotFound",
            "Unit not found.");

    public static readonly Error DuplicateName =
        new(
            "Unit.DuplicateName",
            "A unit with the same Name already exists.");
}