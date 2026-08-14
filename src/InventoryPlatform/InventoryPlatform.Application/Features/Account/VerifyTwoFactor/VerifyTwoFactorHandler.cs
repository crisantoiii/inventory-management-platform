using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.VerifyTwoFactor;

public sealed class VerifyTwoFactorHandler
{
    private readonly IAccountService _accountService;

    public VerifyTwoFactorHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<VerifyTwoFactorResponse>> HandleAsync(
        VerifyTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.VerifyTwoFactorAsync(
            request.UserId,
            request.Code,
            cancellationToken);
    }
}