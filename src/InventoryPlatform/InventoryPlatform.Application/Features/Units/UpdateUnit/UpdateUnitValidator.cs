using FluentValidation;

namespace InventoryPlatform.Application.Features.Units.UpdateUnit;

public sealed class UpdateUnitValidator
    : AbstractValidator<UpdateUnitRequest>
{
    public UpdateUnitValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(10);
    }
}