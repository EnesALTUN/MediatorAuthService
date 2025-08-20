namespace MediatorAuthService.Application.Dtos.ConfigurationDtos;

internal class MediatorTokenOptions
{
    public required List<string> Audience { get; set; }

    public required string Issuer { get; set; }

    public int AccessTokenExpiration { get; set; }

    public int RefreshTokenExpiration { get; set; }

    public required string SecurityKey { get; set; }
}