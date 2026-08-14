namespace InventoryPlatform.Application.Features.Account.VerifyTwoFactor;

public sealed class VerifyTwoFactorResponse
{
    public bool Enabled { get; set; }

    public IReadOnlyList<string> RecoveryCodes { get; init; } =
    Array.Empty<string>();
}