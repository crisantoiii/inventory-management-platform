namespace InventoryPlatform.Application.Features.Account.ConfirmEmail;

public sealed record ConfirmEmailRequest
{
    public Guid UserId { get; init; }

    public string Token { get; init; } = string.Empty;
}