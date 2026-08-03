using InventoryPlatform.Application.Features.Users.ResetPassowrd;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.ResetPassowrd;

public sealed class ResetPasswordHandler
{
    private readonly IIdentityService _identityService;

    public ResetPasswordHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> HandleAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return _identityService.ResetPasswordAsync(request, cancellationToken);
    }
}
