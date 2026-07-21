using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCategoryResponse>> HandleAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _categoryRepository.ExistsByNameAsync(
            request.Name,
            cancellationToken))
        {
            return Result<CreateCategoryResponse>.Failure(
                CategoryErrors.DuplicateName);
        }

        var category = new Category(
                        request.Name, request.Description);

        await _categoryRepository.AddAsync(category, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateCategoryResponse>.Success(
            new CreateCategoryResponse(
                category.Id,
                category.Name));
    }

}