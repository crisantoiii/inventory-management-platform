using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.RequestEmailVerification;

public sealed class RequestEmailVerificationHandler
{
    private readonly IAccountService _accountService;

    public RequestEmailVerificationHandler(
        IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<RequestEmailVerificationResponse>> HandleAsync(
        RequestEmailVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _accountService.RequestEmailVerificationAsync(
            request,
            cancellationToken);
    }
}