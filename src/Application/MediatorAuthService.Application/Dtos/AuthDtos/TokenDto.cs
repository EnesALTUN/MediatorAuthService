namespace MediatorAuthService.Application.Dtos.AuthDtos;

public class TokenDto
{
    public required string AccessToken { get; set; }

    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// UTC expiry of the access token.
    /// </summary>
    public DateTime AccessTokenExpire { get; set; }

    public required string RefreshToken { get; set; }

    /// <summary>
    /// UTC expiry of the refresh token.
    /// </summary>
    public DateTime RefreshTokenExpire { get; set; }
}
