namespace InventoryPlatform.Application.Features.Account.UpdateProfile;

public sealed record UpdateProfileRequest
{
    public Guid UserId { get; init; }

    public string? PhoneNumber { get; init; }
}