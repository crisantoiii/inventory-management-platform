using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetStockMovement;

public sealed class GetStockMovementHandler
{
    private readonly IStockMovementRepository _repository;

    public GetStockMovementHandler(
        IStockMovementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<StockMovementDto>>> HandleAsync(
        GetStockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetStockMovementAsync(
            request.Query,
            request.FromDate,
            request.ToDate,
            request.TransactionType,
            cancellationToken);

        return Result<PagedResult<StockMovementDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<StockMovementDto>>> HandleExportAsync(
        GetStockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = request.Query with
        {
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await _repository.GetStockMovementAsync(
            query,
            request.FromDate,
            request.ToDate,
            request.TransactionType,
            cancellationToken);

        return Result<IReadOnlyList<StockMovementDto>>.Success(result.Items);
    }
}