using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.DisableTwoFactor;

public sealed class DisableTwoFactorHandler
{
    private readonly IAccountService _accountService;

    public DisableTwoFactorHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result> HandleAsync(
        DisableTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.DisableTwoFactorAsync(
            request.UserId,
            cancellationToken);
    }
}