
using InventoryPlatform.Application.DTOs.Role;
using InventoryPlatform.Application.Features.Users;
using InventoryPlatform.Application.Features.Users.CreateUser;
using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Application.Features.Users.ResetPassword;
using InventoryPlatform.Application.Features.Users.UpdateUser;
using InventoryPlatform.Application.Features.Users.UpdateUserRoles;
using InventoryPlatform.Application.Features.Users.UpdateUserStatus;
using InventoryPlatform.Application.Interfaces.Identity;
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
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public IdentityService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<RoleOption>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new RoleOption(role.Name!, false))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            return Result.Failure(
                UserErrors.NotFound(request.Id));
        }

        var token = await _userManager
            .GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            request.Password);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description))));
        }

        return ToResult(result);
    }

    public async Task<Result> UpdateUserStatusAsync(
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            return Result.Failure(
                UserErrors.NotFound(request.Id));
        }

        if(request.IsActive)
        {
            user.LockoutEnd = null;
        }
        else
        {
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> UpdateUserRolesAsync(
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            return Result.Failure(
                UserErrors.NotFound(request.Id));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToAdd = request.Roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(request.Roles).ToList();

        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
                return Result.Failure(Error.None);
        }

        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
                return Result.Failure(Error.None);
        }

        return Result.Success();

    }

    public async Task<Result> UpdateUserAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            return Result.Failure(
                UserErrors.NotFound(request.Id));
        }

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.EmailConfirmed = request.EmailConfirmed;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description))));
        }

        return Result.Success();
    }

    public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is not null)
        {
            return Result<Guid>.Failure(UserErrors.DuplicateUserName);
        }

        user = await _userManager.FindByEmailAsync(request.Email);

        if (user is not null)
        {
            return Result<Guid>.Failure(UserErrors.DuplicateEmail);
        }

        user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = request.EmailConfirmed,
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        var roleResult = await _userManager.AddToRolesAsync(
            user,
            request.Roles);

        return Result<Guid>.Success(user.Id);
    }

    public async Task<Result<GetUserResponse>> GetUserAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        var roles = await _userManager.GetRolesAsync(user);

        var isActive = (user.LockoutEnd is { } lockoutEnd &&
            lockoutEnd > DateTimeOffset.UtcNow) ? false : true;

        return Result<GetUserResponse>.Success(new GetUserResponse
        { 
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Roles = roles.ToList(),
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            IsActive = isActive,
        });
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

        var now = DateTimeOffset.UtcNow;
        query = request.Status switch
        {
            ProductStatusFilter.Active =>
                query.Where(p => p.LockoutEnabled && (p.LockoutEnd == null || p.LockoutEnd <= now)),

            ProductStatusFilter.Inactive =>
                query.Where(p => p.LockoutEnabled && (p.LockoutEnd != null && p.LockoutEnd > now)),

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

    private static Result ToResult(
        IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
        {
            return Result.Success();
        }

        return Result.Failure(
            Error.Validation2(
                string.Join(
                    Environment.NewLine,
                    identityResult.Errors.Select(e => e.Description))));
    }
}