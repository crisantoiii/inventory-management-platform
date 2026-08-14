using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account.GetProfile;

public sealed class GetProfileHandler
{
    private readonly IAccountService _accountService;

    public GetProfileHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public Task<Result<GetProfileResponse>> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _accountService.GetProfileAsync(
            userId,
            cancellationToken);
    }
}