namespace MediatorAuthService.Application.Common.Options;

public class LockoutOptions
{
    public const string SectionName = "Lockout";

    public int MaxFailedAccessAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
