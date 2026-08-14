using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.ConfirmEmail;

public sealed class ConfirmEmailHandler
{
    private readonly IAccountService _accountService;

    public ConfirmEmailHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result> HandleAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.ConfirmEmailAsync(
            request,
            cancellationToken);
    }
}