using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Categories.DeactivateCategory;

public sealed class DeactivateCategoryHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCategoryHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeactivateCategoryResponse>> HandleAsync(
        DeactivateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id,cancellationToken);

        if (category is null)
        {
            return Result<DeactivateCategoryResponse>.Failure(
                CategoryErrors.NotFound);
        }

        category.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeactivateCategoryResponse>.Success(
            new DeactivateCategoryResponse(
                category.Id,
                category.Name,
                category.IsActive));
    }
}