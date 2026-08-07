using FluentValidation;

namespace InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderItemValidator
    : AbstractValidator<CreatePurchaseOrderItemRequest>
{
    public CreatePurchaseOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("A valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0);
    }
}