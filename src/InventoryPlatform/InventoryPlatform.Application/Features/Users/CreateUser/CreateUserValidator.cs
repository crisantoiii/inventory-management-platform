using FluentValidation;

namespace InventoryPlatform.Application.Features.Users.CreateUser;

public sealed class CreateUserValidator
    : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(256);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .MaximumLength(256)
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("The password and confirmation password do not match.");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role must be selected.");
    }
}