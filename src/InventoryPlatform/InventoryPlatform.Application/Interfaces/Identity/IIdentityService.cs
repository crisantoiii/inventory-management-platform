using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<PagedResult<GetUsersResponse>> GetUsersAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);
}