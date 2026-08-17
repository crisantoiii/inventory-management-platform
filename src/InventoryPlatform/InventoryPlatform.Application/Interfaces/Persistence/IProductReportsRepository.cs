using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IProductReportsRepository
{
    Task<PagedResult<ProductReportDto>> GetProductReportsAsync(
        PagedQuery query,
        CancellationToken cancellationToken = default);
}
