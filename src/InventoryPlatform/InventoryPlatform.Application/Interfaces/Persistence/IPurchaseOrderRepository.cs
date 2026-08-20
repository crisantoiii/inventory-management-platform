using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IPurchaseOrderRepository
    : IRepository<PurchaseOrder>
{
    Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(
        string search = "",
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        PurchaseOrderStatus? status = null,
        string? sortBy = null,
        bool descending = false,
        CancellationToken cancellationToken = default);
}
