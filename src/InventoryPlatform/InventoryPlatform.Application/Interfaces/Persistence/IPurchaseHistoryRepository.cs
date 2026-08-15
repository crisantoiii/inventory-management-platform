using InventoryPlatform.Application.DTOs.Reporting;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IPurchaseHistoryRepository
{
    Task<IReadOnlyList<PurchaseHistoryDto>> GetPurchaseHistoryAsync(
        CancellationToken cancellationToken = default);
}