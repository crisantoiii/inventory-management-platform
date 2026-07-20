using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Categories.GetCategory;

public sealed class GetCategoryHandler
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryHandler(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<GetCategoryResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
            return Result<GetCategoryResponse>.Failure(
            CategoryErrors.NotFound);

        return Result<GetCategoryResponse>.Success(new GetCategoryResponse(
                                                    category.Id,
                                                    category.Name,
                                                    category.Description,
                                                    category.IsActive));
    }
}