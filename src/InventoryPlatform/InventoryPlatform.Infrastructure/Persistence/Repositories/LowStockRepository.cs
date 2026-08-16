using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class LowStockRepository : ILowStockRepository
{
    private const decimal LowStockThreshold = 10;

    private readonly ApplicationDbContext _context;

    public LowStockRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LowStockDto>> GetLowStockAsync(
        PagedQuery queryRequest,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products
            .AsNoTracking()
            .Where(x => x.QuantityOnHand <= LowStockThreshold);

        if (!string.IsNullOrWhiteSpace(queryRequest.Search))
        {
            var search = queryRequest.Search.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Name, $"%{search}%") ||
                EF.Functions.Like(x.Sku, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var orderedQuery = ApplySorting(
            query,
            queryRequest);

        var items = await orderedQuery
            .Skip((queryRequest.Page - 1) * queryRequest.PageSize)
            .Take(queryRequest.PageSize)
            .Select(x => new LowStockDto(
                x.Id,
                x.Name,
                x.Sku,
                x.Category.Name,
                x.QuantityOnHand))
            .ToListAsync(cancellationToken);

        return new PagedResult<LowStockDto>
        {
            Items = items,
            Page = queryRequest.Page,
            PageSize = queryRequest.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        PagedQuery request)
    {
        return request.SortBy switch
        {
            LowStockSortFields.ProductName =>
                request.Descending
                    ? query.OrderByDescending(x => x.Name)
                    : query.OrderBy(x => x.Name),

            LowStockSortFields.ProductSku =>
                request.Descending
                    ? query.OrderByDescending(x => x.Sku)
                    : query.OrderBy(x => x.Sku),

            LowStockSortFields.QuantityOnHand =>
                request.Descending
                    ? query.OrderByDescending(x => x.QuantityOnHand)
                    : query.OrderBy(x => x.QuantityOnHand),

            _ =>
                query.OrderBy(x => x.QuantityOnHand)
                    .ThenBy(x => x.Name)
        };
    }
}
