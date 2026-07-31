using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.CreateUser;

public sealed class CreateUserHandler
{

    private readonly IIdentityService _identityService;

    public CreateUserHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result<Guid>> HandleAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return _identityService.CreateUserAsync(request, cancellationToken);
    }

}