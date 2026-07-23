using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetWithRelationshipsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);
}