using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.InventoryTransactions.CreateInventoryTransaction;

public sealed class CreateInventoryTransactionHandler
{
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryTransactionHandler(
        IInventoryTransactionRepository inventoryTransactionRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateInventoryTransactionResponse>> HandleAsync(
        CreateInventoryTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
                    request.ProductId,
                    cancellationToken);

        if (product is null)
        {
            return Result<CreateInventoryTransactionResponse>.Failure(
                InventoryTransactionErrors.ProductNotFound);
        }

        switch (request.TransactionType)
        {
            case TransactionType.StockIn:
                product.IncreaseStock(request.Quantity);
                break;

            case TransactionType.StockOut:

                if(!product.CanDecreaseStock(request.Quantity))
                {
                    return Result<CreateInventoryTransactionResponse>.Failure(
                        InventoryTransactionErrors.InsufficientStock);
                }

                product.DecreaseStock(request.Quantity);
                break;

            case TransactionType.Adjustment:
                product.AdjustStock(request.Quantity);
                break;

            default:
                return Result<CreateInventoryTransactionResponse>.Failure(
                    InventoryTransactionErrors.InvalidTransactionType);
        }

        var inventoryTransaction = new InventoryTransaction(
                        request.ProductId, 
                        request.TransactionType,
                        request.Quantity,
                        request.ReferenceNumber,
                        request.Remarks,
                        request.TransactionDateUtc);

        await _inventoryTransactionRepository.AddAsync(inventoryTransaction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateInventoryTransactionResponse>.Success(
            new CreateInventoryTransactionResponse(
                inventoryTransaction.Id,
                product.QuantityOnHand,
                inventoryTransaction.ReferenceNumber));
    }

}