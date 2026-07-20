using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

public sealed class CategoryRepository
    : Repository<Category>,
      ICategoryRepository
{
    public CategoryRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(
    string name,
    CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Name == name,
            cancellationToken);
    }


    public async Task<PagedResult<Category>> GetPagedActiveAsync(
    PagedQuery request,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = DbSet;

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
                c.Name.Contains(search) ||
                (c.Description != null &&
                 c.Description.Contains(search)));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, request);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Category>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<Category> ApplySorting(
    IQueryable<Category> query,
    PagedQuery request)
    {
        return request.SortBy switch
        {
            CategorySortFields.Name =>
                request.Descending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),

            CategorySortFields.Description =>
                request.Descending
                    ? query.OrderByDescending(c => c.Description)
                    : query.OrderBy(c => c.Description),

            CategorySortFields.IsActive =>
                request.Descending
                    ? query.OrderByDescending(c => c.IsActive)
                    : query.OrderBy(c => c.IsActive),

            _ =>
                query.OrderBy(c => c.Name)

        };
    }
}