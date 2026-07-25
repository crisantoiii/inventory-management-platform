using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

public sealed class InventoryTransactionRepository
    : Repository<InventoryTransaction>,
      IInventoryTransactionRepository
{
    public InventoryTransactionRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task AddAsync(
        InventoryTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(transaction, cancellationToken);
    }

    public async Task<InventoryTransaction?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Product)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }


    public async Task<IReadOnlyList<InventoryTransaction>> GetByProductAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.TransactionDateUtc)
            .ToListAsync(cancellationToken);
    }



    public async Task<PagedResult<InventoryTransaction>> GetPagedAsync(
    PagedQuery request,
    CancellationToken cancellationToken = default)
    {
        IQueryable<InventoryTransaction> query = DbSet
            .Include(x => x.Product)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Product.Name, $"%{search}%") ||
                EF.Functions.Like(x.Product.Sku, $"%{search}%") ||
                (x.ReferenceNumber != null && EF.Functions.Like(x.ReferenceNumber, $"%{search}%")) ||
                (x.Remarks != null && EF.Functions.Like(x.Remarks, $"%{search}%")));

        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, request);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryTransaction>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<InventoryTransaction> ApplySorting(
    IQueryable<InventoryTransaction> query,
    PagedQuery request)
    {
        return request.SortBy switch
        {
            InventoryTransactionSortFields.ProductName =>
                request.Descending
                    ? query.OrderByDescending(x => x.Product.Name)
                    : query.OrderBy(x => x.Product.Name),

            InventoryTransactionSortFields.Quantity =>
                request.Descending
                    ? query.OrderByDescending(x => x.Quantity)
                    : query.OrderBy(x => x.Quantity),

            InventoryTransactionSortFields.TransactionDateUtc =>
                request.Descending
                    ? query.OrderByDescending(x => x.TransactionDateUtc)
                    : query.OrderBy(x => x.TransactionDateUtc),

            InventoryTransactionSortFields.TransactionType =>
                request.Descending
                    ? query.OrderByDescending(x => x.TransactionType)
                    : query.OrderBy(x => x.TransactionType),

            _ =>
                query.OrderByDescending(
                    x => x.TransactionDateUtc)

        };
    }
}