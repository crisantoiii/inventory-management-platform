using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

public sealed class UnitRepository
    : Repository<Unit>,
      IUnitRepository
{
    public UnitRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Code == code,
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
    string name,
    CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Name == name,
            cancellationToken);
    }

    public async Task<bool> ExistsBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Symbol == symbol,
            cancellationToken);
    }

    public async Task<bool> ExistsByCodeNameSymbolAsync(
        string code,
        string name,
        string symbol,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => 
            x.Code == code || 
            x.Name == name ||
            x.Symbol == symbol,
            cancellationToken);
    }


    public async Task<PagedResult<Unit>> GetPagedAsync(
    PagedQuery request,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Unit> query = DbSet;

        query = request.Status switch
        {
            ProductStatusFilter.Active =>
                query.Where(p => p.IsActive),

            ProductStatusFilter.Inactive =>
                query.Where(p => !p.IsActive),

            ProductStatusFilter.All =>
                query,

            _ =>
                query.Where(p => p.IsActive)
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(c =>
                c.Code.Contains(search) ||
                c.Name.Contains(search) ||
                c.Symbol.Contains(search));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, request);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Unit>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<Unit> ApplySorting(
    IQueryable<Unit> query,
    PagedQuery request)
    {
        return request.SortBy switch
        {
            UnitSortFields.Code =>
                request.Descending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code),

            UnitSortFields.Name =>
                request.Descending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),

            UnitSortFields.Symbol =>
                request.Descending
                    ? query.OrderByDescending(c => c.Symbol)
                    : query.OrderBy(c => c.Symbol),

            UnitSortFields.IsActive =>
                request.Descending
                    ? query.OrderByDescending(c => c.IsActive)
                    : query.OrderBy(c => c.IsActive),

            _ =>
                query.OrderBy(c => c.Name)

        };
    }
}