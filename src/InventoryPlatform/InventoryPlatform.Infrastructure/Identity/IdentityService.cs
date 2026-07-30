using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Shared.Paging;
using Microsoft.AspNetCore.Identity;

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

    public Task<Result<PagedResult<GetUsersResponse>>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}