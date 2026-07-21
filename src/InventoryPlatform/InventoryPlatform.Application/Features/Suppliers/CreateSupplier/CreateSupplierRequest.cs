namespace InventoryPlatform.Application.Features.Suppliers.CreateSupplier;

public sealed record CreateSupplierRequest(
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address);