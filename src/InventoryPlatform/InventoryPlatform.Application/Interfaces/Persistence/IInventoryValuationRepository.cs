using InventoryPlatform.Application.DTOs.Reporting;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IInventoryValuationRepository
{
    Task<IReadOnlyList<InventoryValuationDto>> GetInventoryValuationAsync(
        CancellationToken cancellationToken = default);
}