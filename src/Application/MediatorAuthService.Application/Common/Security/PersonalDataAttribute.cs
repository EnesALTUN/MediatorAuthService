namespace MediatorAuthService.Application.Common.Security;

/// <summary>
/// Marks personal data (e.g. e-mail addresses) that is masked rather than fully redacted in logs.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PersonalDataAttribute : Attribute
{ }
