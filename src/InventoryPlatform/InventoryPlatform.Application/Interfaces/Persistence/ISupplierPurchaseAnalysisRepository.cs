using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface ISupplierPurchaseAnalysisRepository
{
    Task<PagedResult<SupplierPurchaseAnalysisDto>> GetSupplierPurchaseAnalysisAsync(
        PagedQuery query,
        DateOnly? fromDate,
        DateOnly? toDate,
        PurchaseOrderStatus? status,
        CancellationToken cancellationToken = default);
}
