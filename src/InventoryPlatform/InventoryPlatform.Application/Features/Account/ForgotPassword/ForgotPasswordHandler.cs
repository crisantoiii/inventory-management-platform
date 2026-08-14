using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.ForgotPassword;

public sealed class ForgotPasswordHandler
{
    private readonly IAccountService _accountService;

    public ForgotPasswordHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result> HandleAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.ForgotPasswordAsync(
            request,
            cancellationToken);
    }
}