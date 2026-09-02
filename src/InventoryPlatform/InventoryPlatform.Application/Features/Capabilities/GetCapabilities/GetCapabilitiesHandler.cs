using InventoryPlatform.Application.Interfaces.Authorization;

namespace InventoryPlatform.Application.Features.Capabilities.GetCapabilities;

public sealed class GetCapabilitiesHandler
{
    private readonly ICapabilityRepository _repository;

    public GetCapabilitiesHandler(
        ICapabilityRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<GetCapabilitiesResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var capabilities = await _repository.GetAllAsync(cancellationToken);

        return capabilities
            .OrderBy(c => c.Name)
            .Select(c => new GetCapabilitiesResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsEnabled = c.IsEnabled
            })
            .ToList();
    }
}
