using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;

public interface IInventoryTransactionRepository
    : IRepository<InventoryTransaction>
{
    Task<PagedResult<InventoryTransaction>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryTransaction>> GetByProductAsync(
        int productId,
        CancellationToken cancellationToken = default);
}