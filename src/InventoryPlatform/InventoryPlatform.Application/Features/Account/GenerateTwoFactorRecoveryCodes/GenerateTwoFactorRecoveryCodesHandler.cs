using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.GenerateTwoFactorRecoveryCodes;

public sealed class GenerateTwoFactorRecoveryCodesHandler
{
    private readonly IAccountService _accountService;

    public GenerateTwoFactorRecoveryCodesHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<GenerateTwoFactorRecoveryCodesResponse>> HandleAsync(
        GenerateTwoFactorRecoveryCodesRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.GenerateTwoFactorRecoveryCodesAsync(
            request.UserId,
            cancellationToken);
    }
}