using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Products.CreateProduct;

public sealed class CreateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _productRepository.ExistsBySkuAsync(
            request.Sku,
            cancellationToken))
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.DuplicateSku);
        }

        var product = new Product(
                        request.Sku,
                        request.Name,
                        request.Unit,
                        request.CostPrice,
                        request.SellingPrice);

        product.ChangeBarcode(request.Barcode);
        product.UpdateDescription(request.Description);

        await _productRepository.AddAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateProductResponse>.Success(
            new CreateProductResponse(
                product.Id,
                product.Sku,
                product.Name));
    }

}