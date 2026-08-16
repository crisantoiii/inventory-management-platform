using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface ILowStockRepository
{
    Task<PagedResult<LowStockDto>> GetLowStockAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);
}
