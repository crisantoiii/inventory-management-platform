using FluentValidation;

namespace InventoryPlatform.Application.Features.Products.CreateProduct;

public sealed class CreateProductValidator
    : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.UnitId)
            .GreaterThan(0)
            .WithMessage("A valid unit must be selected.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("A valid category must be selected.");

        RuleFor(x => x.QuantityOnHand)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0);
    }
}