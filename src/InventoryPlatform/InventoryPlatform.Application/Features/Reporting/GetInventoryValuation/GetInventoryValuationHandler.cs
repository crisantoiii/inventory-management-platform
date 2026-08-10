using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Reporting.GetInventoryValuation;

public sealed class GetInventoryValuationHandler
{
    private readonly IInventoryValuationRepository _repository;

    public GetInventoryValuationHandler(
        IInventoryValuationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<InventoryValuationDto>>> HandleAsync(
        GetInventoryValuationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetInventoryValuationAsync(cancellationToken);

        return Result<IReadOnlyList<InventoryValuationDto>>.Success(result);
    }
}