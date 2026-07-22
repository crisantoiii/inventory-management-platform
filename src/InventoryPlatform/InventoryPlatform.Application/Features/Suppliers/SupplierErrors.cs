using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Suppliers;

public static class SupplierErrors
{
    public static readonly Error NotFound =
        new(
            "Supplier.NotFound",
            "Supplier not found.");

    public static readonly Error DuplicateName =
        new(
            "Supplier.DuplicateName",
            "A supplier with the same Name already exists.");
}