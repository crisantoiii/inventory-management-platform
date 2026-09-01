using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.ManageGroupUsers;

public sealed class ManageGroupUsersHandler
{
    private readonly IAuthorizationGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ManageGroupUsersHandler(
        IAuthorizationGroupRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        ManageGroupUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetWithCapabilitiesAndUsersAsync(
            request.GroupId,
            cancellationToken);

        if (group is null)
        {
            return Result.Failure(
                AuthorizationGroupErrors.NotFound(request.GroupId));
        }

        var currentUserIds = group.UserGroups
            .Select(ug => ug.UserId)
            .ToHashSet();

        var requestedUserIds = request.UserIds
            .ToHashSet();

        var toAdd = requestedUserIds.Except(currentUserIds).ToList();
        var toRemove = currentUserIds.Except(requestedUserIds).ToList();

        foreach (var userId in toAdd)
        {
            group.AssignUser(userId);
        }

        foreach (var userId in toRemove)
        {
            group.RemoveUser(userId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
