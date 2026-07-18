using InventoryPlatform.Application.Interfaces.Persistence;

namespace InventoryPlatform.Application.Features.Products.GetProduct;

public sealed class GetProductHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductResponse?> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
            return null;

        return new GetProductResponse(
            product.Id,
            product.Sku,
            product.Name,
            product.Unit,
            product.CostPrice,
            product.SellingPrice,
            product.Barcode,
            product.Description,
            product.IsActive);
    }
}