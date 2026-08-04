namespace InventoryPlatform.Application.Features.Users.ResetPassword;

public sealed record ResetPasswordRequest
{
    public Guid Id { get; init; }

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;
}