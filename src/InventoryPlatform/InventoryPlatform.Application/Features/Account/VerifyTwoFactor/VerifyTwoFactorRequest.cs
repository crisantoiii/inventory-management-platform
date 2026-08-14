namespace InventoryPlatform.Application.Features.Account.VerifyTwoFactor;

public sealed class VerifyTwoFactorRequest
{
    public Guid UserId { get; set; }

    public string Code { get; set; } = string.Empty;
}