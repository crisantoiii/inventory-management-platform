namespace InventoryPlatform.Application.Features.Account.RequestEmailVerification;

public sealed record RequestEmailVerificationResponse
{
    public bool AlreadyVerified { get; init; }
}