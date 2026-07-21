namespace InventoryPlatform.Application.Features.Suppliers.DeactivateSupplier;

public sealed record DeactivateSupplierResponse(
    int Id,
    string Name,
    bool IsActive);