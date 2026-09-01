using InventoryPlatform.Application.Interfaces.Identity;

namespace InventoryPlatform.Application.Features.Users.GetAllUsers;

public sealed class GetAllUsersHandler
{
    private readonly IIdentityService _identityService;

    public GetAllUsersHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<GetAllUsersResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _identityService.GetAllUsersAsync(cancellationToken);
    }
}
