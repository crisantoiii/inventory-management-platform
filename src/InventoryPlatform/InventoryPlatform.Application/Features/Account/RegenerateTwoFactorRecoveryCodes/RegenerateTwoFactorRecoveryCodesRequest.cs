namespace InventoryPlatform.Application.Features.Account.RegenerateTwoFactorRecoveryCodes;

public sealed class RegenerateTwoFactorRecoveryCodesRequest
{
    public Guid UserId { get; set; }
}