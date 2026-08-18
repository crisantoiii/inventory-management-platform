using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetLowStock;

public sealed class GetLowStockHandler
{
    private readonly ILowStockRepository _repository;

    public GetLowStockHandler(
        ILowStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<LowStockDto>>> HandleAsync(
        GetLowStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetLowStockAsync(
            request.Query,
            cancellationToken);

        return Result<PagedResult<LowStockDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<LowStockDto>>> HandleExportAsync(
        GetLowStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = request.Query with
        {
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await _repository.GetLowStockAsync(
            query,
            cancellationToken);

        return Result<IReadOnlyList<LowStockDto>>.Success(result.Items);
    }
}
