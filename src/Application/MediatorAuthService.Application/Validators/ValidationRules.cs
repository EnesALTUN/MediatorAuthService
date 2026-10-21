using FluentValidation;
using MediatorAuthService.Application.Common.Authorization;

namespace MediatorAuthService.Application.Validators;

public static class ValidationRules
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 100;
    public const int PasswordMaxLength = 128;

    private const string StrongPasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$";

    /// <summary>
    /// The upper bound also keeps PBKDF2 hashing cheap for oversized inputs.
    /// </summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MaximumLength(PasswordMaxLength)
            .Matches(StrongPasswordPattern)
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, digit and special character.");

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Email cannot be empty.")
            .MaximumLength(EmailMaxLength)
            .EmailAddress();

    public static IRuleBuilderOptions<T, string> ValidPermission<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(permission => permission is not null && Permissions.Exists(permission))
            .WithMessage("'{PropertyValue}' is not a known permission.");
}
