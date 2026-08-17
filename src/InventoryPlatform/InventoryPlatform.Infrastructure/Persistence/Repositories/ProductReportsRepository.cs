using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class ProductReportsRepository : IProductReportsRepository
{
    private readonly ApplicationDbContext _context;

    public ProductReportsRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductReportDto>> GetProductReportsAsync(
        PagedQuery queryRequest,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products
            .AsNoTracking();

        query = queryRequest.Status switch
        {
            ProductStatusFilter.Active =>
                query.Where(x => x.IsActive),

            ProductStatusFilter.Inactive =>
                query.Where(x => !x.IsActive),

            ProductStatusFilter.All =>
                query,

            _ =>
                query.Where(x => x.IsActive)
        };

        if (!string.IsNullOrWhiteSpace(queryRequest.Search))
        {
            var search = queryRequest.Search.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Sku, $"%{search}%") ||
                EF.Functions.Like(x.Name, $"%{search}%") ||
                EF.Functions.Like(x.Category.Name, $"%{search}%") ||
                EF.Functions.Like(x.Unit.Name, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var orderedQuery = ApplySorting(
            query,
            queryRequest);

        var items = await orderedQuery
            .Skip((queryRequest.Page - 1) * queryRequest.PageSize)
            .Take(queryRequest.PageSize)
            .Select(x => new ProductReportDto(
                x.Id,
                x.Name,
                x.Sku,
                x.Category != null ? x.Category.Name : null,
                x.Unit != null ? x.Unit.Name : null,
                x.QuantityOnHand,
                x.CostPrice,
                x.SellingPrice,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductReportDto>
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
            ProductReportSortFields.ProductName =>
                request.Descending
                    ? query.OrderByDescending(x => x.Name)
                    : query.OrderBy(x => x.Name),

            ProductReportSortFields.ProductSku =>
                request.Descending
                    ? query.OrderByDescending(x => x.Sku)
                    : query.OrderBy(x => x.Sku),

            ProductReportSortFields.CategoryName =>
                request.Descending
                    ? query.OrderByDescending(x => x.Category.Name)
                    : query.OrderBy(x => x.Category.Name),

            ProductReportSortFields.UnitName =>
                request.Descending
                    ? query.OrderByDescending(x => x.Unit.Name)
                    : query.OrderBy(x => x.Unit.Name),

            ProductReportSortFields.QuantityOnHand =>
                request.Descending
                    ? query.OrderByDescending(x => x.QuantityOnHand)
                    : query.OrderBy(x => x.QuantityOnHand),

            ProductReportSortFields.CostPrice =>
                request.Descending
                    ? query.OrderByDescending(x => x.CostPrice)
                    : query.OrderBy(x => x.CostPrice),

            ProductReportSortFields.SellingPrice =>
                request.Descending
                    ? query.OrderByDescending(x => x.SellingPrice)
                    : query.OrderBy(x => x.SellingPrice),

            ProductReportSortFields.IsActive =>
                request.Descending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

            _ => query
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Sku)
        };
    }
}
