using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetInventoryMovement;

public sealed class GetInventoryMovementHandler
{
    private readonly IInventoryMovementRepository _repository;

    public GetInventoryMovementHandler(
        IInventoryMovementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<InventoryMovementDto>>> HandleAsync(
        GetInventoryMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetInventoryMovementAsync(
            request.Query,
            request.FromDate,
            request.ToDate,
            cancellationToken);

        return Result<PagedResult<InventoryMovementDto>>.Success(result);
    }
}
