using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.DeleteAuthorizationGroup;

public sealed class DeleteAuthorizationGroupHandler
{
    private readonly IAuthorizationGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAuthorizationGroupHandler(
        IAuthorizationGroupRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        DeleteAuthorizationGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetWithCapabilitiesAndUsersAsync(
            request.Id,
            cancellationToken);

        if (group is null)
        {
            return Result.Failure(
                AuthorizationGroupErrors.NotFound(request.Id));
        }

        if (group.UserGroups.Any())
        {
            return Result.Failure(
                AuthorizationGroupErrors.HasAssignedUsers);
        }

        _repository.Remove(group);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
