using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Shared.Guards;

namespace InventoryPlatform.Domain.Entities;

public sealed class AuthorizationGroup : BaseEntity
{
    private readonly List<AuthorizationGroupCapability> _capabilities = new();
    private readonly List<UserAuthorizationGroup> _userGroups = new();

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<AuthorizationGroupCapability> Capabilities =>
        _capabilities.AsReadOnly();

    public IReadOnlyCollection<UserAuthorizationGroup> UserGroups =>
        _userGroups.AsReadOnly();

    private AuthorizationGroup()
    {
    }

    public AuthorizationGroup(string name)
    {
        Rename(name);
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public void AddCapability(Capability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);

        if (capability.Id <= 0)
        {
            throw new DomainException(
                "A persisted capability is required before it can be assigned to a group.");
        }

        if (_capabilities.Any(x => x.CapabilityId == capability.Id))
        {
            return;
        }

        _capabilities.Add(
            AuthorizationGroupCapability.Create(Id, capability.Id));

        capability.AddGroupCapability(_capabilities[^1]);
    }

    public void RemoveCapability(int capabilityId)
    {
        if (capabilityId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capabilityId),
                "Capability ID must be greater than zero.");
        }

        var relationship = _capabilities.SingleOrDefault(
            x => x.CapabilityId == capabilityId);

        if (relationship is null)
        {
            return;
        }

        _capabilities.Remove(relationship);
    }

    public void AssignUser(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (_userGroups.Any(x => x.UserId == userId))
        {
            return;
        }

        _userGroups.Add(
            UserAuthorizationGroup.Create(userId, Id));
    }

    public void RemoveUser(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        var relationship = _userGroups.SingleOrDefault(
            x => x.UserId == userId);

        if (relationship is null)
        {
            return;
        }

        _userGroups.Remove(relationship);
    }
}
