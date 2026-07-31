using InventoryPlatform.Application.Features.Units.GetUnits;
using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Filtering;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
using InventoryPlatform.Shared.Sorting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<PagedResult<GetUsersResponse>> GetUsersAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ApplicationUser> query = _context.Users
            .AsNoTracking();

        query = ApplyFilters(query, request);

        query = ApplySearch(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var orderedQuery = ApplySorting(query, request);

        var users = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = users
            .Select(u => u.Id)
            .ToList();

        var roleLookup = await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId)
            select new
            {
                userRole.UserId,
                role.Name
            })
            .ToListAsync(cancellationToken);

        var rolesByUser = roleLookup
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g
                    .Select(x => x.Name!)
                    .OrderBy(name => name)
                    .ToList());

        var items = users
            .Select(user => new GetUsersResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                Roles = rolesByUser.TryGetValue(user.Id, out var roles) ? roles : []
            })
            .ToList();

        return new PagedResult<GetUsersResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<ApplicationUser> ApplyFilters(
    IQueryable<ApplicationUser> query,
    PagedQuery request)
    {
        // Future:
        // - Role filtering
        // - Lockout filtering


        query = request.Status switch
        {
            ProductStatusFilter.Active =>
                query.Where(p => p.LockoutEnabled),

            ProductStatusFilter.Inactive =>
                query.Where(p => !p.LockoutEnabled),

            ProductStatusFilter.All =>
                query,

            _ =>
                query.Where(p => p.LockoutEnabled)
        };


        return query;
    }

    private static IOrderedQueryable<ApplicationUser> ApplySorting(
        IQueryable<ApplicationUser> query,
        PagedQuery request)
    {

        return request.SortBy switch
        {
            UserSortFields.UserName => request.Descending
                ? query.OrderByDescending(u => u.UserName)
                : query.OrderBy(u => u.UserName),

            UserSortFields.Email => request.Descending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            UserSortFields.Lockout => request.Descending
                ? query.OrderByDescending(u => u.LockoutEnd)
                : query.OrderBy(u => u.LockoutEnd),

            _ => query.OrderBy(u => u.UserName)
        };
    }

    private static IQueryable<ApplicationUser> ApplySearch(
    IQueryable<ApplicationUser> query,
    PagedQuery request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return query;

        var search = request.Search.Trim();

        return query.Where(user =>
            (user.UserName != null && user.UserName.Contains(search)) ||
            (user.Email != null && user.Email.Contains(search)));
    }
}