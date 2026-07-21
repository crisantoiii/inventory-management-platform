using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Features.Suppliers.GetSuppliers;

public static class SuppliersSortFields
{
    public const string Name = nameof(Supplier.Name);
    public const string ContactPerson = nameof(Supplier.ContactPerson);
    public const string Email = nameof(Supplier.Email);
    public const string Phone = nameof(Supplier.Phone);
    public const string Address = nameof(Supplier.Address);

    public const string IsActive = nameof(Product.IsActive);
}