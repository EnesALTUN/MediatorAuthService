using FluentValidation;
using MediatorAuthService.Application.Cqrs.Queries.AuthQueries;

namespace MediatorAuthService.Application.Validators.AuthValidators;

public class CreateTokenValidator : AbstractValidator<CreateTokenQuery>
{
	public CreateTokenValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress();

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password cannot be empty.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$")
				.WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, digit and special character.");
	}
}