using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.RegenerateTwoFactorRecoveryCodes;

public sealed class RegenerateTwoFactorRecoveryCodesHandler
{
    private readonly IAccountService _accountService;

    public RegenerateTwoFactorRecoveryCodesHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<RegenerateTwoFactorRecoveryCodesResponse>> HandleAsync(
        RegenerateTwoFactorRecoveryCodesRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.RegenerateTwoFactorRecoveryCodesAsync(
            request.UserId,
            cancellationToken);
    }
}