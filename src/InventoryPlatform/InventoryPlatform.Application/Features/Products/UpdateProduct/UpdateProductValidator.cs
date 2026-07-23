using FluentValidation;

namespace InventoryPlatform.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductValidator
    : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("A valid unit must be selected.");

        RuleFor(x => x.UnitId)
            .GreaterThan(0)
            .WithMessage("A valid unit must be selected.");

        RuleFor(x => x.QuantityOnHand)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0);
    }
}