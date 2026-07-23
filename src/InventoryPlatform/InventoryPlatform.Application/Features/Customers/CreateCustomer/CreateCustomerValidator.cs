using FluentValidation;

namespace InventoryPlatform.Application.Features.Customers.CreateCustomer;

public sealed class CreateCustomertValidator
    : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomertValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}