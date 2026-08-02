using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.GetUser;

public sealed class GetUserHandler
{
    private readonly IIdentityService _identityService;

    public GetUserHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result<GetUserResponse>> HandleAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        return _identityService.GetUserAsync(id, cancellationToken);
    }
}