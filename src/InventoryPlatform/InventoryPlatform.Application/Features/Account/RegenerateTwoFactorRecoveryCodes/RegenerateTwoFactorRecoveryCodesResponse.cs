namespace InventoryPlatform.Application.Features.Account.RegenerateTwoFactorRecoveryCodes;

public sealed class RegenerateTwoFactorRecoveryCodesResponse
{
    public IReadOnlyList<string> RecoveryCodes { get; init; } =
        Array.Empty<string>();
}