using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Products.GetProducts;

public sealed record GetProductsRequest : PagedRequest
{
    public string? Search { get; init; }
}