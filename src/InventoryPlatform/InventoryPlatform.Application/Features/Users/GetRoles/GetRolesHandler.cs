using InventoryPlatform.Application.DTOs.Role;
using InventoryPlatform.Application.Interfaces.Identity;

namespace InventoryPlatform.Application.Features.Users.GetRoles;

public sealed class GetRolesHandler
{
    private readonly IIdentityService _identityService;

    public GetRolesHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<RoleOption>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _identityService.GetRolesAsync(cancellationToken);
    }
}