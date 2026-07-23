using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Products.GetProduct;

public sealed class GetProductHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetWithRelationshipsAsync(
            id,
            cancellationToken);

        if (product is null)
            return Result<GetProductResponse>.Failure(
            ProductErrors.NotFound);

        return Result<GetProductResponse>.Success(new GetProductResponse(
                                                    product.Id,
                                                    product.Sku,
                                                    product.Name,
                                                    product.Category.Name,
                                                    product.Unit.Name,
                                                    product.CategoryId,
                                                    product.UnitId,
                                                    product.QuantityOnHand,
                                                    product.CostPrice,
                                                    product.SellingPrice,
                                                    product.Barcode,
                                                    product.Description,
                                                    product.IsActive));
    }
}