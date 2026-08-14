namespace InventoryPlatform.Application.Features.Account.ForgotPassword;

public sealed record ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}