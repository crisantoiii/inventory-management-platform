using FluentValidation;

namespace InventoryPlatform.Application.Features.Suppliers.CreateSupplier;

public sealed class CreateSuppliertValidator
    : AbstractValidator<CreateSupplierRequest>
{
    public CreateSuppliertValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}