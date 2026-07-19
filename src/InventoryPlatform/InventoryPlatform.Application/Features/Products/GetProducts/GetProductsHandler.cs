using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
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

    public async Task<Result<PagedResult<GetProductsResponse>>> HandleAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Search = request.Search,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            Descending = request.Descending
        };

        var products = await _productRepository.GetPagedActiveAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetProductsResponse>
        {
            Items = products.Items
                .Select(product => new GetProductsResponse(
                    product.Id,
                    product.Sku,
                    product.Name,
                    product.CostPrice,
                    product.SellingPrice,
                    product.IsActive))
                .ToList(),

            Page = products.Page,
            PageSize = products.PageSize,
            TotalCount = products.TotalCount
        };

        return Result<PagedResult<GetProductsResponse>>.Success(response);
    }
}