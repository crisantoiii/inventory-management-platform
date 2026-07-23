using FluentValidation;

namespace InventoryPlatform.Application.Features.Customers.UpdateCustomer;

public sealed class UpdateCustomerValidator
    : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}