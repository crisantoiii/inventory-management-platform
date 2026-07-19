using InventoryPlatform.Application.Features.Products.GetProducts;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
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

    public async Task<PagedResult<Product>> GetPagedActiveAsync(
    PagedQuery request,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query =
            DbSet.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(p =>
                p.Sku.Contains(search) ||
                p.Name.Contains(search));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, request);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<Product> ApplySorting(
    IQueryable<Product> query,
    PagedQuery request)
    {
        return request.SortBy switch
        {
            ProductSortFields.Sku => request.Descending
                ? query.OrderByDescending(p => p.Sku)
                : query.OrderBy(p => p.Sku),

            ProductSortFields.Name => request.Descending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            ProductSortFields.CostPrice => request.Descending
                ? query.OrderByDescending(p => p.CostPrice)
                : query.OrderBy(p => p.CostPrice),

            ProductSortFields.SellingPrice => request.Descending
                ? query.OrderByDescending(p => p.SellingPrice)
                : query.OrderBy(p => p.SellingPrice),

            ProductSortFields.IsActive => request.Descending
                ? query.OrderByDescending(p => p.IsActive)
                : query.OrderBy(p => p.IsActive),

            _ => query.OrderBy(p => p.Name)
        };
    }
}