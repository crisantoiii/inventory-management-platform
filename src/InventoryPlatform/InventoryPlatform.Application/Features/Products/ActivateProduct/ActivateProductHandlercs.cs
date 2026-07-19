using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Products.ActivateProduct;

public sealed class ActivateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivateProductResponse>> HandleAsync(
        ActivateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);

        if (product is null)
        {
            return Result<ActivateProductResponse>.Failure(
                ProductErrors.NotFound);
        }

        product.Activate();

        await _unitOfWork.SaveChangesAsync();

        return Result<ActivateProductResponse>.Success(
            new ActivateProductResponse(
                product.Id,
                product.Name,
                product.IsActive));
    }
}