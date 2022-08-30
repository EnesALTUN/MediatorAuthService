using FluentValidation;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;

namespace MediatorAuthService.Application.Validators.AuthValidators;

public class CreateTokenValidator : AbstractValidator<CreateTokenQuery>
{
	public CreateTokenValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty();

		RuleFor(x => x.Password)
			.NotEmpty();
	}
}