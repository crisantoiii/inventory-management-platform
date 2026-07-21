namespace InventoryPlatform.Application.Features.Suppliers.ActivateSupplier;

public sealed record ActivateSupplierResponse(
    int Id,
    string Name,
    bool IsActive);