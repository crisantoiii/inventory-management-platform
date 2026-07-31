namespace InventoryPlatform.Application.Features.Users.UpdateUser;

public sealed record UpdateUserRequest
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }

    public bool EmailConfirmed { get; init; }
}