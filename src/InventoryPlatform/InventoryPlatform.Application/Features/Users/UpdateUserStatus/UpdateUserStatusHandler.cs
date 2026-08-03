using InventoryPlatform.Application.Interfaces.Identity;

namespace InventoryPlatform.Application.Features.Users.UpdateUserStatus;

public sealed class UpdateUserStatusHandler
{
    private readonly IIdentityService _identityService;

    public UpdateUserStatusHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Shared.Results.Result> HandleAsync(
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        return _identityService.UpdateUserStatusAsync(
            request, 
            cancellationToken);
    }

}