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
        IQueryable<ApplicationUser> query = _context.Users.AsNoTracking();

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
}