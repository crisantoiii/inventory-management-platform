namespace InventoryPlatform.Application.Features.Account.GenerateTwoFactorRecoveryCodes;

public sealed class GenerateTwoFactorRecoveryCodesResponse
{
    public IReadOnlyList<string> RecoveryCodes { get; init; } =
        Array.Empty<string>();
}