using Microsoft.AspNetCore.Identity;

namespace InventoryPlatform.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; init; } = true;
}