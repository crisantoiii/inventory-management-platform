namespace InventoryPlatform.Application.Features.Account.GenerateTwoFactorRecoveryCodes;

public sealed class GenerateTwoFactorRecoveryCodesRequest
{
    public Guid UserId { get; set; }
}