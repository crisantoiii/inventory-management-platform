namespace InventoryPlatform.Application.Features.Account.RequestEmailVerification;

public sealed record RequestEmailVerificationRequest
{
    public Guid UserId { get; init; }
}