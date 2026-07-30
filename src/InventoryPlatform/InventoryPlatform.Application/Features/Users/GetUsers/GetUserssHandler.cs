using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.GetUsers;

public sealed class GetUsersHandler
{
    private readonly IIdentityService _identityService;

    public GetUsersHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result<PagedResult<GetUsersResponse>>> HandleAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        return _identityService.GetUsersAsync(
            request,
            cancellationToken);
    }
}