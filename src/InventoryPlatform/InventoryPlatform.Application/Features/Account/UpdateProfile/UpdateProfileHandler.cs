using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.UpdateProfile;

public sealed class UpdateProfileHandler
{
    private readonly IAccountService _accountService;

    public UpdateProfileHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result> HandleAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.UpdateProfileAsync(
            request,
            cancellationToken);
    }
}