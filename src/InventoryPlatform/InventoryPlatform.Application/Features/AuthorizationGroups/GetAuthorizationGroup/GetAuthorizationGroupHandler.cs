using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroup;

public sealed class GetAuthorizationGroupHandler
{
    private readonly IAuthorizationGroupRepository _repository;
    private readonly ICapabilityRepository _capabilityRepository;
    private readonly IIdentityService _identityService;

    public GetAuthorizationGroupHandler(
        IAuthorizationGroupRepository repository,
        ICapabilityRepository capabilityRepository,
        IIdentityService identityService)
    {
        _repository = repository;
        _capabilityRepository = capabilityRepository;
        _identityService = identityService;
    }

    public async Task<Result<GetAuthorizationGroupResponse>> HandleAsync(
        GetAuthorizationGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _repository.GetWithCapabilitiesAndUsersAsync(
            request.Id,
            cancellationToken);

        if (group is null)
        {
            return Result<GetAuthorizationGroupResponse>.Failure(
                AuthorizationGroupErrors.NotFound(request.Id));
        }

        var allCapabilities = await _capabilityRepository.GetAllAsync(cancellationToken);
        var capDict = allCapabilities.ToDictionary(c => c.Id);

        var allUsers = await _identityService.GetAllUsersAsync(cancellationToken);

        var userIdsInGroup = group.UserGroups
            .Select(ug => ug.UserId)
            .ToHashSet();

        var usersInGroup = allUsers
            .Where(u => userIdsInGroup.Contains(u.Id))
            .Select(u => new UserSummary
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email
            })
            .ToList();

        var capabilities = group.Capabilities
            .Where(gc => capDict.ContainsKey(gc.CapabilityId))
            .Select(gc => new CapabilitySummary
            {
                Id = gc.CapabilityId,
                Name = capDict[gc.CapabilityId].Name,
                IsEnabled = capDict[gc.CapabilityId].IsEnabled
            })
            .ToList();

        return Result<GetAuthorizationGroupResponse>.Success(
            new GetAuthorizationGroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Capabilities = capabilities,
                Users = usersInGroup
            });
    }
}
