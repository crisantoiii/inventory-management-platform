using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class StockCountRepository
    : Repository<StockCountSession>, IStockCountRepository
{
    public StockCountRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<StockCountSession?> GetSessionWithLinesAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<StockCountSession?> GetSessionWithLinesForUpdateAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<StockCountSession?> GetSessionByProductAndDateAsync(
        DateOnly countDate,
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(
                s => s.CountDate == countDate && s.Lines.Any(l => l.ProductId == productId),
                cancellationToken);
    }

    public async Task<bool> ExistsSessionForProductAsync(
        DateOnly countDate,
        int productId,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(s => s.CountDate == countDate && s.Lines.Any(l => l.ProductId == productId));

        if (excludeSessionId.HasValue)
        {
            query = query.Where(s => s.Id != excludeSessionId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<StockCountSession>> GetPagedAsync(
        PagedQuery query,
        StockCountStatus? status = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var sessions = DbSet.AsNoTracking();

        if (status.HasValue)
        {
            sessions = sessions.Where(s => s.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            sessions = sessions.Where(s => s.CountDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            sessions = sessions.Where(s => s.CountDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            sessions = sessions.Where(s => s.Notes != null && s.Notes.Contains(search));
        }

        var totalCount = await sessions.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(sessions, query);

        var items = await orderedQuery
            .Skip((query.PageNum - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<StockCountSession>
        {
            Items = items,
            Page = query.PageNum,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<StockCountSession> ApplySorting(
        IQueryable<StockCountSession> query,
        PagedQuery request)
    {
        if (request.SortBy is null)
            return query.OrderByDescending(s => s.CountDate);

        return request.SortBy switch
        {
            "CountDate" => request.Descending
                ? query.OrderByDescending(s => s.CountDate)
                : query.OrderBy(s => s.CountDate),
            "Status" => request.Descending
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),
            "CreatedAtUtc" => request.Descending
                ? query.OrderByDescending(s => s.CreatedAtUtc)
                : query.OrderBy(s => s.CreatedAtUtc),
            _ => query.OrderByDescending(s => s.CountDate)
        };
    }

    public async Task<IReadOnlyList<StockCountSession>> GetUnreconciledSessionsForProductAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(s => s.Status != StockCountStatus.Posted && s.Status != StockCountStatus.Cancelled)
            .Include(s => s.Lines)
            .Where(s => s.Lines.Any(l => l.ProductId == productId && l.CountedQuantity.HasValue))
            .ToListAsync(cancellationToken);
    }
}