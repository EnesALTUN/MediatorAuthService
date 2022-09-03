using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;

namespace MediatorAuthService.Application.Validators.AuthValidators;

public class CreateTokenByRefreshTokenValidator : AbstractValidator<CreateTokenByRefreshTokenCommand>
{
	public CreateTokenByRefreshTokenValidator()
	{
		RuleFor(x => x.RefreshToken)
			.NotEmpty();
	}
}