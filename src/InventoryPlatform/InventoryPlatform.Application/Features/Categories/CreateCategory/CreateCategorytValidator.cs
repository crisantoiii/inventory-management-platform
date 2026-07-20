using FluentValidation;

namespace InventoryPlatform.Application.Features.Categories.CreateCategory;

public sealed class CreateCategorytValidator
    : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategorytValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}