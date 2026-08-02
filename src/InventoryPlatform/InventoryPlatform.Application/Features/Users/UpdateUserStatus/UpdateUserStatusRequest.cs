namespace InventoryPlatform.Application.Features.Users.UpdateUserStatus;

public sealed record UpdateUserStatusRequest
{
    public Guid Id { get; init; }

    public bool IsActive { get; init; }
}