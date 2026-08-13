using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.GetTwoFactorStatus;

public sealed class GetTwoFactorStatusHandler
{
    private readonly IAccountService _accountService;

    public GetTwoFactorStatusHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<GetTwoFactorStatusResponse>> HandleAsync(
        GetTwoFactorStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.GetTwoFactorStatusAsync(
            request.UserId,
            cancellationToken);
    }
}