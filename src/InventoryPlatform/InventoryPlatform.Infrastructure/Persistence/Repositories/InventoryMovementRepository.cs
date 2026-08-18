using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryMovementRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<InventoryMovementDto>> GetInventoryMovementAsync(
        PagedQuery queryRequest,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default)
    {
        var products = _context.Products
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryRequest.Search))
        {
            var search = queryRequest.Search.Trim();

            products = products.Where(x =>
                EF.Functions.Like(x.Name, $"%{search}%") ||
                EF.Functions.Like(x.Sku, $"%{search}%"));
        }

        var fromDateUtc = fromDate?.ToDateTime(TimeOnly.MinValue);
        var toDateExclusiveUtc = toDate?.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var transactionQuery = _context.InventoryTransactions
            .AsNoTracking();

        var query =
            from product in products
            let stockInQuantity = transactionQuery
                .Where(x =>
                    x.ProductId == product.Id &&
                    x.TransactionType == TransactionType.StockIn &&
                    (!fromDateUtc.HasValue || x.TransactionDateUtc >= fromDateUtc.Value) &&
                    (!toDateExclusiveUtc.HasValue || x.TransactionDateUtc < toDateExclusiveUtc.Value))
                .Select(x => (decimal?)x.Quantity)
                .Sum() ?? 0m
            let stockOutQuantity = transactionQuery
                .Where(x =>
                    x.ProductId == product.Id &&
                    x.TransactionType == TransactionType.StockOut &&
                    (!fromDateUtc.HasValue || x.TransactionDateUtc >= fromDateUtc.Value) &&
                    (!toDateExclusiveUtc.HasValue || x.TransactionDateUtc < toDateExclusiveUtc.Value))
                .Select(x => (decimal?)x.Quantity)
                .Sum() ?? 0m
            let adjustmentQuantity = transactionQuery
                .Where(x =>
                    x.ProductId == product.Id &&
                    x.TransactionType == TransactionType.Adjustment &&
                    (!fromDateUtc.HasValue || x.TransactionDateUtc >= fromDateUtc.Value) &&
                    (!toDateExclusiveUtc.HasValue || x.TransactionDateUtc < toDateExclusiveUtc.Value))
                .Select(x => (decimal?)x.Quantity)
                .Sum() ?? 0m
            let movementFromPeriodStart = transactionQuery
                .Where(x =>
                    x.ProductId == product.Id &&
                    (!fromDateUtc.HasValue || x.TransactionDateUtc >= fromDateUtc.Value))
                .Select(x => (decimal?)(
                    x.TransactionType == TransactionType.StockIn
                        ? x.Quantity
                        : x.TransactionType == TransactionType.StockOut
                            ? -x.Quantity
                            : x.Quantity))
                .Sum() ?? 0m
            let movementAfterPeriod = transactionQuery
                .Where(x =>
                    x.ProductId == product.Id &&
                    toDateExclusiveUtc.HasValue &&
                    x.TransactionDateUtc >= toDateExclusiveUtc.Value)
                .Select(x => (decimal?)(
                    x.TransactionType == TransactionType.StockIn
                        ? x.Quantity
                        : x.TransactionType == TransactionType.StockOut
                            ? -x.Quantity
                            : x.Quantity))
                .Sum() ?? 0m
            select new InventoryMovementRow
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,

                OpeningQuantity =
                    product.QuantityOnHand -
                    movementFromPeriodStart,

                StockInQuantity = stockInQuantity,

                StockOutQuantity = stockOutQuantity,

                AdjustmentQuantity = adjustmentQuantity,

                ClosingQuantity =
                    product.QuantityOnHand -
                    movementAfterPeriod
            };

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(
            query,
            queryRequest);

        var items = await orderedQuery
            .Skip((queryRequest.Page - 1) * queryRequest.PageSize)
            .Take(queryRequest.PageSize)
            .Select(x => new InventoryMovementDto(
                x.ProductId,
                x.ProductName,
                x.ProductSku,
                x.OpeningQuantity,
                x.StockInQuantity,
                x.StockOutQuantity,
                x.AdjustmentQuantity,
                x.ClosingQuantity))
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryMovementDto>
        {
            Items = items,
            Page = queryRequest.Page,
            PageSize = queryRequest.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<InventoryMovementRow> ApplySorting(
        IQueryable<InventoryMovementRow> query,
        PagedQuery request)
    {
        return request.SortBy switch
        {
            InventoryMovementSortFields.ProductName =>
                request.Descending
                    ? query.OrderByDescending(x => x.ProductName)
                    : query.OrderBy(x => x.ProductName),

            InventoryMovementSortFields.ProductSku =>
                request.Descending
                    ? query.OrderByDescending(x => x.ProductSku)
                    : query.OrderBy(x => x.ProductSku),

            InventoryMovementSortFields.OpeningQuantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.OpeningQuantity)
                    : query.OrderBy(x => x.OpeningQuantity),

            InventoryMovementSortFields.StockInQuantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.StockInQuantity)
                    : query.OrderBy(x => x.StockInQuantity),

            InventoryMovementSortFields.StockOutQuantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.StockOutQuantity)
                    : query.OrderBy(x => x.StockOutQuantity),

            InventoryMovementSortFields.AdjustmentQuantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.AdjustmentQuantity)
                    : query.OrderBy(x => x.AdjustmentQuantity),

            InventoryMovementSortFields.ClosingQuantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.ClosingQuantity)
                    : query.OrderBy(x => x.ClosingQuantity),

            _ => query
                .OrderBy(x => x.ProductName)
                .ThenBy(x => x.ProductSku)
        };
    }

    private sealed class InventoryMovementRow
    {
        public int ProductId { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public string ProductSku { get; init; } = string.Empty;

        public decimal OpeningQuantity { get; init; }

        public decimal StockInQuantity { get; init; }

        public decimal StockOutQuantity { get; init; }

        public decimal AdjustmentQuantity { get; init; }

        public decimal ClosingQuantity { get; init; }
    }
}
