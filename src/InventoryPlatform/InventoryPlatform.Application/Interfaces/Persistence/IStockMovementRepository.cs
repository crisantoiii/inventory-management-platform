using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IStockMovementRepository
{
    Task<PagedResult<StockMovementDto>> GetStockMovementAsync(
        PagedQuery query,
        DateOnly? fromDate,
        DateOnly? toDate,
        TransactionType? transactionType,
        CancellationToken cancellationToken = default);
}