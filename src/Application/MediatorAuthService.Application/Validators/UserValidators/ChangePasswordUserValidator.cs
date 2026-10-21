using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;

namespace MediatorAuthService.Application.Validators.UserValidators;

public class ChangePasswordUserValidator : AbstractValidator<ChangePasswordUserCommand>
{
    public ChangePasswordUserValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .MaximumLength(ValidationRules.PasswordMaxLength);

        RuleFor(x => x.NewPassword)
            .StrongPassword()
            .NotEqual(x => x.OldPassword).WithMessage("The new password must be different from the old password.");
    }
}
