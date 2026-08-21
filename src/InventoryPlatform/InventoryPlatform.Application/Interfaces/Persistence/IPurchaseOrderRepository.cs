using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IPurchaseOrderRepository
    : IRepository<PurchaseOrder>
{
    Task<PagedResult<PurchaseOrder>> GetPurchaseOrdersAsync(
        PagedQuery query,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        PurchaseOrderStatus? status = null,
        CancellationToken cancellationToken = default);
}
