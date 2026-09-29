using FluentValidation;

namespace InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderValidator
    : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .WithMessage("A valid supplier must be selected.");

        RuleFor(x => x.ExpectedDeliveryDate)
            .NotEmpty();

        RuleFor(x => x.Remarks)
            .MaximumLength(500);

        RuleForEach(x => x.Items)
            .SetValidator(new CreatePurchaseOrderItemValidator());
    }
}
