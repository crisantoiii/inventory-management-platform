using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;

public sealed class CustomerRepository
    : Repository<Customer>,
      ICustomerRepository
{
    public CustomerRepository(
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

    public async Task<bool> ExistsByNameAsync(
        int excludingCustomerId,
        string name,
    CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Name == name && x.Id != excludingCustomerId,
            cancellationToken);
    }


    public async Task<PagedResult<Customer>> GetPagedAsync(
    PagedQuery request,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = DbSet;

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
                (c.ContactPerson != null &&
                 c.ContactPerson.Contains(search)) ||
                 (c.Email != null &&
                 c.Email.Contains(search)));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, request);

        var items = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IOrderedQueryable<Customer> ApplySorting(
    IQueryable<Customer> query,
    PagedQuery request)
    {
        return request.SortBy switch
        {
            SupplierSortFields.Name =>
                request.Descending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),

            SupplierSortFields.ContactPerson =>
                request.Descending
                    ? query.OrderByDescending(c => c.ContactPerson)
                    : query.OrderBy(c => c.ContactPerson),

            SupplierSortFields.Email =>
                request.Descending
                    ? query.OrderByDescending(c => c.Email)
                    : query.OrderBy(c => c.Email),

            SupplierSortFields.Phone =>
                request.Descending
                    ? query.OrderByDescending(c => c.Phone)
                    : query.OrderBy(c => c.Phone),

            SupplierSortFields.IsActive =>
                request.Descending
                    ? query.OrderByDescending(c => c.IsActive)
                    : query.OrderBy(c => c.IsActive),

            _ =>
                query.OrderBy(c => c.Name)

        };
    }
}