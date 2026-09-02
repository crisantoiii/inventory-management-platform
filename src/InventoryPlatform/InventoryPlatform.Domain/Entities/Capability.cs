using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Shared.Guards;

namespace InventoryPlatform.Domain.Entities;

public sealed class Capability : BaseEntity
{
    private readonly List<AuthorizationGroupCapability> _groupCapabilities = new();

    public string Name { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public IReadOnlyCollection<AuthorizationGroupCapability> GroupCapabilities =>
        _groupCapabilities.AsReadOnly();

    private Capability()
    {
    }

    public Capability(string name)
    {
        Rename(name);
        IsEnabled = true;
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    internal void AddGroupCapability(AuthorizationGroupCapability groupCapability)
    {
        ArgumentNullException.ThrowIfNull(groupCapability);

        if (groupCapability.CapabilityId != Id)
        {
            throw new DomainException(
                "The capability relationship does not belong to this capability.");
        }

        if (_groupCapabilities.Contains(groupCapability))
        {
            return;
        }

        _groupCapabilities.Add(groupCapability);
    }

    internal void RemoveGroupCapability(AuthorizationGroupCapability groupCapability)
    {
        ArgumentNullException.ThrowIfNull(groupCapability);
        _groupCapabilities.Remove(groupCapability);
    }
}
