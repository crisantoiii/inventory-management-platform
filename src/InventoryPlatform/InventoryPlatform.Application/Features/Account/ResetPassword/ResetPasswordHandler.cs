using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.ResetPassword;

public sealed class ResetPasswordHandler
{
    private readonly IAccountService _accountService;

    public ResetPasswordHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result> HandleAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.ResetPasswordAsync(
            request,
            cancellationToken);
    }
}