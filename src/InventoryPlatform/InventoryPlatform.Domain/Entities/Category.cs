using InventoryPlatform.Domain.Common;
using InventoryPlatform.Shared.Guards;

namespace InventoryPlatform.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    private Category() { }

    public Category(
        string name,
        string? description)
    {
        Rename(name);
        Description = description;
        IsActive = true;
    }

    public void Update(
        string name,
        string? description)
    {
        Rename(name);
        Description = description;
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}