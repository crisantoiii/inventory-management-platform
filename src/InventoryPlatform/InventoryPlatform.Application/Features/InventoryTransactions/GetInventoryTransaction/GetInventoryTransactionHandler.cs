using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransaction;

public sealed class GetInventoryTransactionHandler
{
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

    public GetInventoryTransactionHandler(
        IInventoryTransactionRepository inventoryTransactionRepository)
    {
        _inventoryTransactionRepository = inventoryTransactionRepository;
    }

    public async Task<Result<GetInventoryTransactionResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var inventoryTransaction = await _inventoryTransactionRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (inventoryTransaction is null)
            return Result<GetInventoryTransactionResponse>.Failure(
            InventoryTransactionErrors.NotFound);

        return Result<GetInventoryTransactionResponse>.Success(new GetInventoryTransactionResponse(
                                                    inventoryTransaction.Id,
                                                    inventoryTransaction.ProductId,
                                                    inventoryTransaction.Product.Name,
                                                    inventoryTransaction.Product.Sku,
                                                    inventoryTransaction.TransactionType,
                                                    inventoryTransaction.Quantity,
                                                    inventoryTransaction.ReferenceNumber,
                                                    inventoryTransaction.Remarks,
                                                    inventoryTransaction.TransactionDateUtc));
    }
}