using System.Text;

namespace MediatorAuthService.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "JwtTokenOption";

    /// <summary>
    /// HMAC-SHA256 requires a key of at least 256 bits.
    /// </summary>
    public const int MinimumSecurityKeyBytes = 32;

    public List<string> Audience { get; set; } = [];

    public string Issuer { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 15;

    public int RefreshTokenExpirationDays { get; set; } = 30;

    public string SecurityKey { get; set; } = string.Empty;

    internal IEnumerable<string> Validate()
    {
        if (Audience.Count == 0 || Audience.Any(string.IsNullOrWhiteSpace))
            yield return $"{SectionName}:Audience must contain at least one non-empty value.";

        if (string.IsNullOrWhiteSpace(Issuer))
            yield return $"{SectionName}:Issuer is required.";

        if (Encoding.UTF8.GetByteCount(SecurityKey) < MinimumSecurityKeyBytes)
            yield return $"{SectionName}:SecurityKey must be at least {MinimumSecurityKeyBytes} bytes long.";

        if (AccessTokenExpirationMinutes <= 0)
            yield return $"{SectionName}:AccessTokenExpirationMinutes must be greater than zero.";

        if (RefreshTokenExpirationDays <= 0)
            yield return $"{SectionName}:RefreshTokenExpirationDays must be greater than zero.";
    }
}
