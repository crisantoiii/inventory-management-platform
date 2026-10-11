using FluentValidation;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryHandler
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateCategoryRequest> _validator;

    public UpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateCategoryRequest> validator)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<UpdateCategoryResponse>> HandleAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var firstFailure = validationResult.Errors[0];
            return Result<UpdateCategoryResponse>.Failure(
                CategoryErrors.Validation(firstFailure.ErrorMessage));
        }

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