using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

namespace MediatorAuthService.Application.Validators.AuthValidators;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail();

        // The password policy is intentionally not checked here: it would disclose the policy and lock out
        // accounts created under an older one.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MaximumLength(ValidationRules.PasswordMaxLength);
    }
}
