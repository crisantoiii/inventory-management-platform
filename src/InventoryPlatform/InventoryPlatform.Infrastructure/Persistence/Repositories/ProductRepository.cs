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
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = DbSet
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Sku.Contains(search) ||
                p.Name.Contains(search));
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}