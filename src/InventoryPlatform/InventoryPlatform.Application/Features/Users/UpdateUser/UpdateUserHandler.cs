using InventoryPlatform.Application.Features.Users.CreateUser;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.UpdateUser;

public sealed class UpdateUserHandler
{

    private readonly IIdentityService _identityService;

    public UpdateUserHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> HandleAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        return _identityService.UpdateUserAsync(request, cancellationToken);
    }

}