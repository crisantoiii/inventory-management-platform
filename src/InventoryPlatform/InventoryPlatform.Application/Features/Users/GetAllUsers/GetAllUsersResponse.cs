namespace InventoryPlatform.Application.Features.Users.GetAllUsers;

public sealed record GetAllUsersResponse
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}
