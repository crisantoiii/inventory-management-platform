using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository
    : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            x => x.Sku == sku,
            cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Sku == sku,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(
    CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}