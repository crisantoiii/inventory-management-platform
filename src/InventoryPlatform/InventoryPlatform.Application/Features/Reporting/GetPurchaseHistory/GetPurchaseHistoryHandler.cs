using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
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

    public async Task<Result<IReadOnlyList<PurchaseHistoryDto>>> HandleAsync(
        GetPurchaseHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPurchaseHistoryAsync(
            cancellationToken);

        return Result<IReadOnlyList<PurchaseHistoryDto>>.Success(result);
    }
}