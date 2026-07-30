namespace InventoryPlatform.Application.Features.Users.GetUsers;

public sealed record GetUsersResponse
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool EmailConfirmed { get; init; }

    public bool LockoutEnabled { get; init; }

    public DateTimeOffset? LockoutEnd { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];
}