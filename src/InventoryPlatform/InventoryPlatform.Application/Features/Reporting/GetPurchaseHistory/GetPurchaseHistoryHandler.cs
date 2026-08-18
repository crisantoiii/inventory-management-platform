using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetPurchaseHistory;

public sealed class GetPurchaseHistoryHandler
{
    private readonly IPurchaseHistoryRepository _repository;

    public GetPurchaseHistoryHandler(
        IPurchaseHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<PurchaseHistoryDto>>> HandleAsync(
        GetPurchaseHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Search = request.Search,
            SortBy = request.SortBy,
            Descending = request.Descending
        };

        var result = await _repository.GetPurchaseHistoryAsync(
            query,
            request.FromDate,
            request.ToDate,
            request.PurchaseOrderStatus,
            cancellationToken);

        return Result<PagedResult<PurchaseHistoryDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PurchaseHistoryDto>>> HandleExportAsync(
        GetPurchaseHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Page = 1,
            PageSize = int.MaxValue,
            Search = request.Search,
            SortBy = request.SortBy,
            Descending = request.Descending
        };

        var result = await _repository.GetPurchaseHistoryAsync(
            query,
            request.FromDate,
            request.ToDate,
            request.PurchaseOrderStatus,
            cancellationToken);

        return Result<IReadOnlyList<PurchaseHistoryDto>>.Success(result.Items);
    }
}