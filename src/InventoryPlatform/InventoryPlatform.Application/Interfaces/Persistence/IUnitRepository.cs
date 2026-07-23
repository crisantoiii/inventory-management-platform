using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;

public interface IUnitRepository
    : IRepository<Unit>
{
    Task<PagedResult<Unit>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySymbolAsync(
        string Symbol,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeNameSymbolAsync(
        string code,
        string name,
        string symbol,
        CancellationToken cancellationToken = default);
}