using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.Domain.Entities;

public sealed class UserAuthorizationGroup : BaseEntity
{
    public Guid UserId { get; private set; }

    public int AuthorizationGroupId { get; private set; }

    private UserAuthorizationGroup()
    {
    }

    private UserAuthorizationGroup(
        Guid userId,
        int authorizationGroupId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        if (authorizationGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(authorizationGroupId),
                "Authorization group ID must be greater than zero.");
        }

        UserId = userId;
        AuthorizationGroupId = authorizationGroupId;
    }

    internal static UserAuthorizationGroup Create(
        Guid userId,
        int authorizationGroupId)
    {
        return new UserAuthorizationGroup(
            userId,
            authorizationGroupId);
    }
}
