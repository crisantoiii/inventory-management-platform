using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class AuthorizationGroupRepository
    : Repository<AuthorizationGroup>,
      IAuthorizationGroupRepository
{
    public AuthorizationGroupRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<AuthorizationGroup?> GetWithCapabilitiesAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .Include(x => x.Capabilities)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthorizationGroup>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.UserGroups.Any(ug => ug.UserId == userId))
            .Include(x => x.Capabilities)
            .ToListAsync(cancellationToken);
    }
}
