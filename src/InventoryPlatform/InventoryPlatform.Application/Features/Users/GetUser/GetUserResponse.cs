namespace InventoryPlatform.Application.Features.Users.GetUser;

public sealed record GetUserResponse
{
    public Guid Id { get; init; }

    public string? Username { get; init; } = string.Empty;

    public string? Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];

    public bool EmailConfirmed { get; init; }

    public string? PhoneNumber { get; init; } = string.Empty;

    public bool PhoneNumberConfirmed { get; init; }

    public bool LockoutEnabled { get; init; }

    public DateTimeOffset? LockoutEnd { get; init; }

    public int AccessFailedCount { get; init; }
}
    
