namespace InventoryPlatform.Application.Features.Account.SetupTwoFactor;

public sealed class SetupTwoFactorResponse
{
    public string AuthenticatorKey { get; set; } = string.Empty;
}