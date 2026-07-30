using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
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
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ApplicationUser> query = _context.Users
            .AsNoTracking();

        query = ApplyFilters(query, request);

        query = ApplySearch(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = users
            .Select(user => new GetUsersResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                Roles = []
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
    GetUsersRequest request)
    {
        // Future:
        // - Role filtering
        // - Lockout filtering
        return query;
    }

    private static IQueryable<ApplicationUser> ApplySearch(
    IQueryable<ApplicationUser> query,
    GetUsersRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return query;

        var search = request.Search.Trim();

        return query.Where(user =>
            (user.UserName != null && user.UserName.Contains(search)) ||
            (user.Email != null && user.Email.Contains(search)));
    }
}