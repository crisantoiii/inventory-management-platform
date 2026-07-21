using InventoryPlatform.Application.Features.Categories.ActivateCategory;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Categories.ActivateCategory;

public sealed class ActivateCategoryHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCategoryHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivateCategoryResponse>> HandleAsync(
        ActivateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);

        if (category is null)
        {
            return Result<ActivateCategoryResponse>.Failure(
                CategoryErrors.NotFound);
        }

        category.Activate();

        await _unitOfWork.SaveChangesAsync();

        return Result<ActivateCategoryResponse>.Success(
            new ActivateCategoryResponse(
                category.Id,
                category.Name,
                category.IsActive));
    }
}