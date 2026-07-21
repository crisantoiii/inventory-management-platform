using FluentValidation;

namespace InventoryPlatform.Application.Features.Suppliers.UpdateSupplier;

public sealed class UpdateSupplierValidator
    : AbstractValidator<UpdateSupplierRequest>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}