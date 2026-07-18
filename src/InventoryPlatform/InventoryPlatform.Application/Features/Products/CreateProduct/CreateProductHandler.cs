using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;

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

    public async Task<CreateProductResponse> HandleAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _productRepository.ExistsBySkuAsync(
        request.Sku,
        cancellationToken))
        {
            throw new InvalidOperationException(
                $"Product with SKU '{request.Sku}' already exists.");
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

        return new CreateProductResponse(
                    product.Id,
                    product.Sku,
                    product.Name);
    }

}