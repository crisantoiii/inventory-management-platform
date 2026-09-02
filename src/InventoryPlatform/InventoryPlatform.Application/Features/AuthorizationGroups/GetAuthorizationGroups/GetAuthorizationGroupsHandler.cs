using InventoryPlatform.Application.Interfaces.Authorization;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.GetAuthorizationGroups;

public sealed class GetAuthorizationGroupsHandler
{
    private readonly IAuthorizationGroupRepository _repository;

    public GetAuthorizationGroupsHandler(
        IAuthorizationGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<GetAuthorizationGroupsResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await _repository.GetAllWithDetailsAsync(cancellationToken);

        return groups
            .OrderBy(g => g.Name)
            .Select(g => new GetAuthorizationGroupsResponse
            {
                Id = g.Id,
                Name = g.Name,
                CapabilityCount = g.Capabilities.Count,
                UserCount = g.UserGroups.Count
            })
            .ToList();
    }
}
