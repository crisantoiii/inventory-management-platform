using FluentValidation;

namespace InventoryPlatform.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryValidator
    : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}