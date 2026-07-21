using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateCategoryResponse>> HandleAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
        {
            return Result<UpdateCategoryResponse>.Failure(
                        CategoryErrors.NotFound);
        }

        category.Update(request.Name, request.Description);

        _categoryRepository.Update(category);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateCategoryResponse>.Success(
            new UpdateCategoryResponse(
                category.Id,
                category.Name));
    }
}