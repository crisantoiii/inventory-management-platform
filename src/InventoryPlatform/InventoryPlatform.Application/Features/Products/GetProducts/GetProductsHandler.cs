using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Products.GetProducts;

public sealed class GetProductsHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(
        IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<GetProductsResponse>>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetActiveAsync(cancellationToken);

        var response = products
            .Select(product => new GetProductsResponse(
                product.Id,
                product.Sku,
                product.Name,
                product.CostPrice,
                product.SellingPrice,
                product.IsActive))
            .ToList();

        return Result<IReadOnlyList<GetProductsResponse>>.Success(response);
    }
}