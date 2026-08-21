using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;

public sealed class ReceivePurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReceivePurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IProductRepository productRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _productRepository = productRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReceivePurchaseOrderResponse>> HandleAsync(
        ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<ReceivePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            return Result<ReceivePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.ProductNotFound(request.ProductId));
        }

        purchaseOrder.Receive(
            request.ProductId,
            request.Quantity);

        product.IncreaseStock(request.Quantity);

        var inventoryTransaction = new InventoryTransaction(
            request.ProductId,
            TransactionType.StockIn,
            request.Quantity,
            $"PO-{purchaseOrder.Id}",
            $"Purchase Order {purchaseOrder.Id} receiving",
            DateTime.UtcNow);

        await _inventoryTransactionRepository.AddAsync(
            inventoryTransaction,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ReceivePurchaseOrderResponse>.Success(
            new ReceivePurchaseOrderResponse(
                purchaseOrder.Id,
                purchaseOrder.Status));
    }
}