namespace MediatorAuthService.Application.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// E-mail addresses are stored trimmed and lower-cased so lookups do not depend on database collation.
    /// </summary>
    public static string NormalizeEmail(this string email) => email.Trim().ToLowerInvariant();
}
