using FluentValidation;

namespace InventoryPlatform.Application.Features.AuthorizationGroups.CreateAuthorizationGroup;

public sealed class CreateAuthorizationGroupValidator
    : AbstractValidator<CreateAuthorizationGroupRequest>
{
    public CreateAuthorizationGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
