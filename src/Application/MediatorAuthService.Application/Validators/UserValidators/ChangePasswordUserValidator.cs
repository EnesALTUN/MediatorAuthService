using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;

namespace MediatorAuthService.Application.Validators.UserValidators;

public class ChangePasswordUserValidator : AbstractValidator<ChangePasswordUserCommand>
{
	public ChangePasswordUserValidator()
	{
		RuleFor(x => x.OldPassword)
			.NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty();
    }
}