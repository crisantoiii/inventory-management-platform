using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}