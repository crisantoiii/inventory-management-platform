using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.UpdateUserRoles;

public sealed class UpdateUserRolesHandler
{

    private readonly IIdentityService _identityService;

    public UpdateUserRolesHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> HandleAsync(
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        return _identityService.UpdateUserRolesAsync(request, cancellationToken);
    }

}