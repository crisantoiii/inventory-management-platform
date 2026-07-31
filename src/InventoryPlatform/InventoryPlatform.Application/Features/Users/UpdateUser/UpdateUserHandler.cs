using InventoryPlatform.Application.Interfaces.Identity;

namespace InventoryPlatform.Application.Features.Users.UpdateUser;

public sealed class UpdateUserHandler
{

    private readonly IIdentityService _identityService;

    public UpdateUserHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

}