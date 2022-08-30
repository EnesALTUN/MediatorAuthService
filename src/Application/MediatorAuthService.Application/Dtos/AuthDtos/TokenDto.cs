namespace MediatorAuthService.Application.Dtos.AuthDtos;

public class TokenDto
{
    public string AccessToken { get; set; }

    public DateTime AccessTokenExpire { get; set; }
}