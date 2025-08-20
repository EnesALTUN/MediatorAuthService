namespace MediatorAuthService.Application.Dtos.AuthDtos;

public class TokenDto
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public DateTime AccessTokenExpire { get; set; }

    public DateTime RefreshTokenExpire { get; set; }
}