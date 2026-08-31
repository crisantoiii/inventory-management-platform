using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class CapabilityRepository
    : Repository<Capability>,
      ICapabilityRepository
{
    public CapabilityRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public Task<Capability?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Name == name, cancellationToken);
    }
}
