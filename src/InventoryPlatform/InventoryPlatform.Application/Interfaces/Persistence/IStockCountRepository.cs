using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IStockCountRepository
    : IRepository<StockCountSession>
{
    Task<StockCountSession?> GetSessionWithLinesAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<StockCountSession?> GetSessionWithLinesForUpdateAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<StockCountSession?> GetSessionByProductAndDateAsync(
        DateOnly countDate,
        int productId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsSessionForProductAsync(
        DateOnly countDate,
        int productId,
        int? excludeSessionId = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StockCountSession>> GetPagedAsync(
        PagedQuery query,
        StockCountStatus? status = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockCountSession>> GetUnreconciledSessionsForProductAsync(
        int productId,
        CancellationToken cancellationToken = default);
}