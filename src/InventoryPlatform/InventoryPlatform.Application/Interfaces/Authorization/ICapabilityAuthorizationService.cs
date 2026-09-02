namespace InventoryPlatform.Application.Interfaces.Authorization;

public interface ICapabilityAuthorizationService
{
    Task<bool> HasCapabilityAsync(
        Guid userId,
        string capabilityName,
        CancellationToken cancellationToken = default);
}
