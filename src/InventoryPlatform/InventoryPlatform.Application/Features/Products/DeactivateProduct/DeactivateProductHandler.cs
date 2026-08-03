using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Products.DeactivateProduct;

public sealed class DeactivateProductHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeactivateProductResponse>> HandleAsync(
        DeactivateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result<DeactivateProductResponse>.Failure(
                ProductErrors.NotFound);
        }

        product.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeactivateProductResponse>.Success(
            new DeactivateProductResponse(
                product.Id,
                product.Name,
                product.IsActive));
    }
}