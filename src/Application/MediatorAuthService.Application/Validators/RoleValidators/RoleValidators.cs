using FluentValidation;
using MediatorAuthService.Application.Cqrs.Commands.RoleCommands;

namespace MediatorAuthService.Application.Validators.RoleValidators;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationRules.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Permissions)
            .NotNull();

        RuleForEach(x => x.Permissions)
            .ValidPermission();
    }
}

public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ValidationRules.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Permissions)
            .NotNull();

        RuleForEach(x => x.Permissions)
            .ValidPermission();
    }
}
