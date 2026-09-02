using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Interfaces.Authorization;

public interface IAuthorizationGroupRepository
    : IRepository<AuthorizationGroup>
{
    Task<AuthorizationGroup?> GetWithCapabilitiesAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthorizationGroup>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<AuthorizationGroup?> GetWithCapabilitiesAndUsersAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthorizationGroup>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default);
}
