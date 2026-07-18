using InventoryPlatform.Application.Interfaces.Persistence;

namespace InventoryPlatform.Application.Features.Products.GetProducts;

public sealed class GetProductsHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<GetProductsResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetActiveAsync(
            cancellationToken);

        return products
            .Select(product => new GetProductsResponse(
                product.Id,
                product.Sku,
                product.Name,
                product.CostPrice,
                product.SellingPrice,
                product.IsActive))
            .ToList();
    }
}