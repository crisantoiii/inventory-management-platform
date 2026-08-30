using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Exceptions;

namespace InventoryPlatform.Domain.Entities;

public sealed class AuthorizationGroupCapability : BaseEntity
{
    public int AuthorizationGroupId { get; private set; }

    public int CapabilityId { get; private set; }

    private AuthorizationGroupCapability()
    {
    }

    private AuthorizationGroupCapability(
        int authorizationGroupId,
        int capabilityId)
    {
        if (authorizationGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(authorizationGroupId),
                "Authorization group ID must be greater than zero.");
        }

        if (capabilityId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capabilityId),
                "Capability ID must be greater than zero.");
        }

        AuthorizationGroupId = authorizationGroupId;
        CapabilityId = capabilityId;
    }

    internal static AuthorizationGroupCapability Create(
        int authorizationGroupId,
        int capabilityId)
    {
        return new AuthorizationGroupCapability(
            authorizationGroupId,
            capabilityId);
    }
}
