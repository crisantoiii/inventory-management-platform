using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IPurchaseOrderRepository
    : IRepository<PurchaseOrder>
{
    Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(
        string search = "",
        CancellationToken cancellationToken = default);
}