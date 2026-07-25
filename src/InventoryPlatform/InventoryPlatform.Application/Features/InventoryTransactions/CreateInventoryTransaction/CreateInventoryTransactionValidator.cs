using FluentValidation;

namespace InventoryPlatform.Application.Features.InventoryTransactions.CreateInventoryTransaction;

public sealed class CreateInventoryTransactionValidator
    : AbstractValidator<CreateInventoryTransactionRequest>
{
    public CreateInventoryTransactionValidator()
    {

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");
    }
}