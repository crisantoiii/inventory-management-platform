using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetSupplierPurchaseAnalysis;

public sealed class GetSupplierPurchaseAnalysisHandler
{
    private readonly ISupplierPurchaseAnalysisRepository _repository;

    public GetSupplierPurchaseAnalysisHandler(
        ISupplierPurchaseAnalysisRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<SupplierPurchaseAnalysisDto>>> HandleAsync(
        GetSupplierPurchaseAnalysisRequest request,
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

        var result = await _repository.GetSupplierPurchaseAnalysisAsync(
            query,
            request.FromDate,
            request.ToDate,
            request.PurchaseOrderStatus,
            cancellationToken);

        return Result<PagedResult<SupplierPurchaseAnalysisDto>>.Success(result);
    }
}
