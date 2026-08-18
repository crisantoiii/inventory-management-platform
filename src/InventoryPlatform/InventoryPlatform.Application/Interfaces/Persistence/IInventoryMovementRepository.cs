using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IInventoryMovementRepository
{
    Task<PagedResult<InventoryMovementDto>> GetInventoryMovementAsync(
        PagedQuery query,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default);
}
