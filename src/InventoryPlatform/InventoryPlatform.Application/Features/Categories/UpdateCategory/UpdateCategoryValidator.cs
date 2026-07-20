using FluentValidation;

namespace InventoryPlatform.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryValidator
    : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}