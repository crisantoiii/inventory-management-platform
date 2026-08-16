using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class StockMovementRepository : IStockMovementRepository
{
    private readonly ApplicationDbContext _context;

    public StockMovementRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<StockMovementDto>> GetStockMovementAsync(
        PagedQuery queryRequest,
        DateOnly? fromDate,
        DateOnly? toDate,
        TransactionType? transactionType,
        CancellationToken cancellationToken = default)
    {
        IQueryable<InventoryTransaction> query = _context.InventoryTransactions
            .AsNoTracking()
            .Include(x => x.Product);

        if (fromDate.HasValue)
        {
            var fromDateUtc = fromDate.Value.ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.TransactionDateUtc >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateExclusiveUtc = toDate.Value
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.TransactionDateUtc < toDateExclusiveUtc);
        }

        if (transactionType.HasValue)
        {
            query = query.Where(x =>
                x.TransactionType == transactionType.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryRequest.Search))
        {
            var search = queryRequest.Search.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Product.Name, $"%{search}%") ||
                EF.Functions.Like(x.Product.Sku, $"%{search}%") ||
                EF.Functions.Like(x.ReferenceNumber, $"%{search}%") ||
                (x.Remarks != null &&
                 EF.Functions.Like(x.Remarks, $"%{search}%")));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var orderedQuery = ApplySorting(
            query,
            queryRequest);

        var items = await orderedQuery
            .Skip((queryRequest.Page - 1) * queryRequest.PageSize)
            .Take(queryRequest.PageSize)
            .Select(x => new StockMovementDto(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.Product.Sku,
                x.TransactionType,
                x.Quantity,
                x.ReferenceNumber,
                x.Remarks,
                x.TransactionDateUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<StockMovementDto>
        {
            Items = items,
            Page = queryRequest.Page,
            PageSize = queryRequest.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<InventoryTransaction> ApplySorting(
        IQueryable<InventoryTransaction> query,
        PagedQuery request)
    {
        return request.SortBy switch
        {
            StockMovementSortFields.ProductName =>
                request.Descending
                    ? query.OrderByDescending(x => x.Product.Name)
                    : query.OrderBy(x => x.Product.Name),

            StockMovementSortFields.ProductSku =>
                request.Descending
                    ? query.OrderByDescending(x => x.Product.Sku)
                    : query.OrderBy(x => x.Product.Sku),

            StockMovementSortFields.TransactionType =>
                request.Descending
                    ? query.OrderByDescending(x => x.TransactionType)
                    : query.OrderBy(x => x.TransactionType),

            StockMovementSortFields.Quantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.Quantity)
                    : query.OrderBy(x => x.Quantity),

            StockMovementSortFields.ReferenceNumber =>
                request.Descending
                    ? query.OrderByDescending(x => x.ReferenceNumber)
                    : query.OrderBy(x => x.ReferenceNumber),

            StockMovementSortFields.TransactionDateUtc =>
                request.Descending
                    ? query.OrderByDescending(x => x.TransactionDateUtc)
                    : query.OrderBy(x => x.TransactionDateUtc),

            _ =>
                query.OrderByDescending(
                    x => x.TransactionDateUtc)
        };
    }
}