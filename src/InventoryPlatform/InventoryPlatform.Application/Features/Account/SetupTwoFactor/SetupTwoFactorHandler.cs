using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.SetupTwoFactor;

public sealed class SetupTwoFactorHandler
{
    private readonly IAccountService _accountService;

    public SetupTwoFactorHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<SetupTwoFactorResponse>> HandleAsync(
        SetupTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.SetupTwoFactorAsync(
            request.UserId,
            cancellationToken);
    }
}