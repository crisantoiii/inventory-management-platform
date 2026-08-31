using InventoryPlatform.Application.Interfaces.Authorization;

namespace InventoryPlatform.Application.Authorization;

public sealed class CapabilityAuthorizationService
    : ICapabilityAuthorizationService
{
    private readonly ICapabilityRepository _capabilityRepository;
    private readonly IAuthorizationGroupRepository _authorizationGroupRepository;

    public CapabilityAuthorizationService(
        ICapabilityRepository capabilityRepository,
        IAuthorizationGroupRepository authorizationGroupRepository)
    {
        _capabilityRepository = capabilityRepository;
        _authorizationGroupRepository = authorizationGroupRepository;
    }

    public async Task<bool> HasCapabilityAsync(
        Guid userId,
        string capabilityName,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(capabilityName))
        {
            return false;
        }

        var capability = await _capabilityRepository.GetByNameAsync(
            capabilityName,
            cancellationToken);

        if (capability is null || !capability.IsEnabled)
        {
            return false;
        }

        var groups = await _authorizationGroupRepository.GetForUserAsync(
            userId,
            cancellationToken);

        return groups.Any(group =>
            group.Capabilities.Any(
                groupCapability => groupCapability.CapabilityId == capability.Id));
    }
}
