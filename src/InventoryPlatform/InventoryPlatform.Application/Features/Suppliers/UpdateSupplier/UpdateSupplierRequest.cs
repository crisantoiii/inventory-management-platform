namespace InventoryPlatform.Application.Features.Suppliers.UpdateSupplier;

public sealed record UpdateSupplierRequest(
    int Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address);