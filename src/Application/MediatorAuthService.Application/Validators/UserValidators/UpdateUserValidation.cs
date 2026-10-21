using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;

namespace MediatorAuthService.Application.Validators.UserValidators;

public class UpdateUserValidation : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationRules.NameMaxLength);

        RuleFor(x => x.Surname)
            .NotEmpty()
            .MaximumLength(ValidationRules.NameMaxLength);

        RuleFor(x => x.Email)
            .ValidEmail();
    }
}
