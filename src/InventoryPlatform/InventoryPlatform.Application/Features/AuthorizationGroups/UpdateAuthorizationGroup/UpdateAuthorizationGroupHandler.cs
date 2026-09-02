using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.UpdateAuthorizationGroup;

public sealed class UpdateAuthorizationGroupHandler
{
    private readonly IAuthorizationGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAuthorizationGroupHandler(
        IAuthorizationGroupRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UpdateAuthorizationGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (group is null)
        {
            return Result.Failure(
                AuthorizationGroupErrors.NotFound(request.Id));
        }

        if (await _repository.ExistsAsync(
            x => x.Name == request.Name && x.Id != request.Id,
            cancellationToken))
        {
            return Result.Failure(
                AuthorizationGroupErrors.DuplicateName);
        }

        group.Rename(request.Name);

        _repository.Update(group);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
