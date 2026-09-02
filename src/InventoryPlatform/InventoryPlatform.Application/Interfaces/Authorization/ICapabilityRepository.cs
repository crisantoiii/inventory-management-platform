using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Application.Interfaces.Persistence;

namespace InventoryPlatform.Application.Interfaces.Authorization;

public interface ICapabilityRepository : IRepository<Capability>
{
    Task<Capability?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}
