using FluentValidation;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePurchaseOrderRequest> _validator;

    public CreatePurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreatePurchaseOrderRequest> validator)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<CreatePurchaseOrderResponse>> HandleAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var firstFailure = validationResult.Errors[0];

            return Result<CreatePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.Validation(firstFailure.ErrorMessage));
        }

        var supplier = await _supplierRepository.GetByIdAsync(
            request.SupplierId,
            cancellationToken);

        if (supplier is null)
        {
            return Result<CreatePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.SupplierNotFound);
        }

        if (!supplier.IsActive)
        {
            return Result<CreatePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.SupplierInactive);
        }

        var purchaseOrder = PurchaseOrder.Create(
            request.SupplierId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            request.ExpectedDeliveryDate,
            request.Remarks);

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                item.ProductId,
                cancellationToken);

            if (product is null)
            {
                return Result<CreatePurchaseOrderResponse>.Failure(
                    PurchaseOrderErrors.ProductNotFound(item.ProductId));
            }

            if (!product.IsActive)
            {
                return Result<CreatePurchaseOrderResponse>.Failure(
                    PurchaseOrderErrors.ProductInactive(item.ProductId));
            }

            purchaseOrder.AddItem(
                item.ProductId,
                item.Quantity,
                item.UnitCost);
        }

        await _purchaseOrderRepository.AddAsync(
            purchaseOrder,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreatePurchaseOrderResponse>.Success(
            new CreatePurchaseOrderResponse(
                purchaseOrder.Id,
                purchaseOrder.Status));
    }
}