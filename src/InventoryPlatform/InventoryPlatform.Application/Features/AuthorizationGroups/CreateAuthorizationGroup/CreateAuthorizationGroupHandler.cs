using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.CreateAuthorizationGroup;

public sealed class CreateAuthorizationGroupHandler
{
    private readonly IAuthorizationGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAuthorizationGroupHandler(
        IAuthorizationGroupRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateAuthorizationGroupResponse>> HandleAsync(
        CreateAuthorizationGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsAsync(
            x => x.Name == request.Name,
            cancellationToken))
        {
            return Result<CreateAuthorizationGroupResponse>.Failure(
                AuthorizationGroupErrors.DuplicateName);
        }

        var group = new AuthorizationGroup(request.Name);

        await _repository.AddAsync(group, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateAuthorizationGroupResponse>.Success(
            new CreateAuthorizationGroupResponse(
                group.Id,
                group.Name));
    }
}
