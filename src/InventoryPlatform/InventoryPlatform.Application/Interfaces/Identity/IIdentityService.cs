using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<Result<PagedResult<GetUsersResponse>>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default);
}