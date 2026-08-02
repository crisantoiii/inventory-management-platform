namespace InventoryPlatform.Application.Features.Users.CreateUser;

public sealed record CreateUserRequest
{
    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;

    public List<string> Roles { get; init; } = [];

    public bool EmailConfirmed { get; init; }
}