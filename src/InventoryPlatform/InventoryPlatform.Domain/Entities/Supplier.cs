using InventoryPlatform.Domain.Common;
using InventoryPlatform.Shared.Guards;

public sealed class Supplier : BaseEntity
{
    private Supplier() { }

    public Supplier(
        string name,
        string? contactPerson,
        string? email,
        string? phone,
        string? address)
    {
        Name = name;
        ContactPerson = contactPerson;
        Email = email;
        Phone = phone;
        Address = address;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string? ContactPerson { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Address { get; private set; }

    public bool IsActive { get; private set; }

    public void Update(
        string name,
        string? contactPerson,
        string? email,
        string? phone,
        string? address)
    {
        Name = name;
        ContactPerson = contactPerson;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}