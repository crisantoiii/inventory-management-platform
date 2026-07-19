using InventoryPlatform.Application.Features.Products.GetProduct;
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
        GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetActiveAsync(
                                        request.Search,
                                        cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            products = products
                .Where(p =>
                    p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

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