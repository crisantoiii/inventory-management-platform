using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.UpdateUserStatus;

public sealed class UpdateUserStatusHandler
{
    private readonly IIdentityService _identityService;

    public UpdateUserStatusHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> HandleAsync(
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        return _identityService.UpdateUserStatusAsync(
            request, 
            cancellationToken);
    }

}