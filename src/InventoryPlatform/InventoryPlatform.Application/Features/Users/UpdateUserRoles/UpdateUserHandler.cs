using InventoryPlatform.Application.Interfaces.Identity;

namespace InventoryPlatform.Application.Features.Users.UpdateUserRoles;

public sealed class UpdateUserRolesHandler
{

    private readonly IIdentityService _identityService;

    public UpdateUserRolesHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

}