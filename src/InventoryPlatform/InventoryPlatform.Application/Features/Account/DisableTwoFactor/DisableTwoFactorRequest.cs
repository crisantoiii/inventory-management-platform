namespace InventoryPlatform.Application.Features.Account.DisableTwoFactor;

public sealed class DisableTwoFactorRequest
{
    public Guid UserId { get; set; }
}