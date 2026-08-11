using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.ChangePassword;

public sealed class ChangePasswordHandler
{
    private readonly IAccountService _accountService;

    public ChangePasswordHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result> HandleAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.ChangePasswordAsync(
            request,
            cancellationToken);
    }
}