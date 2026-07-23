using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateProductResponse>> HandleAsync(
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
        {
            return Result<UpdateProductResponse>.Failure(
                        ProductErrors.NotFound);
        }

        product.Rename(request.Name);
        product.UpdateDescription(request.Description);
        product.ChangeBarcode(request.Barcode);
        product.ChangeCostPrice(request.CostPrice);
        product.ChangeSellingPrice(request.SellingPrice);
        product.ChangeUnit(request.UnitId);
        product.ChangeCategory(request.CategoryId);
        product.ChangeQuantityOnHand(request.QuantityOnHand);

        _productRepository.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateProductResponse>.Success(
            new UpdateProductResponse(
                product.Id,
                product.Name));
    }
}