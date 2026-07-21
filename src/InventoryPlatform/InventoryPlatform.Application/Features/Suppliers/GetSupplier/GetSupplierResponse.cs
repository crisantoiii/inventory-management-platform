namespace InventoryPlatform.Application.Features.Suppliers.GetSupplier;

public sealed record GetSupplierResponse(
    int Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);