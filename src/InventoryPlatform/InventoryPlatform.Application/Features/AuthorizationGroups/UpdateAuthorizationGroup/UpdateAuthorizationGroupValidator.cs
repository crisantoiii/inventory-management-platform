using FluentValidation;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.UpdateAuthorizationGroup;

public sealed class UpdateAuthorizationGroupValidator
    : AbstractValidator<UpdateAuthorizationGroupRequest>
{
    public UpdateAuthorizationGroupValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
