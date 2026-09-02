using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.ManageGroupCapabilities;

public sealed class ManageGroupCapabilitiesHandler
{
    private readonly IAuthorizationGroupRepository _repository;
    private readonly ICapabilityRepository _capabilityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ManageGroupCapabilitiesHandler(
        IAuthorizationGroupRepository repository,
        ICapabilityRepository capabilityRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _capabilityRepository = capabilityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        ManageGroupCapabilitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetWithCapabilitiesAsync(
            request.GroupId,
            cancellationToken);

        if (group is null)
        {
            return Result.Failure(
                AuthorizationGroupErrors.NotFound(request.GroupId));
        }

        var currentCapIds = group.Capabilities
            .Select(c => c.CapabilityId)
            .ToHashSet();

        var requestedCapIds = request.CapabilityIds
            .ToHashSet();

        var toAdd = requestedCapIds.Except(currentCapIds).ToList();
        var toRemove = currentCapIds.Except(requestedCapIds).ToList();

        foreach (var capId in toAdd)
        {
            var capability = await _capabilityRepository.GetByIdAsync(
                capId,
                cancellationToken);

            if (capability is null)
            {
                return Result.Failure(
                    AuthorizationGroupErrors.CapabilityNotFound(capId));
            }

            group.AddCapability(capability);
        }

        foreach (var capId in toRemove)
        {
            group.RemoveCapability(capId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
