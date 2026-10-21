using MediatorAuthService.Domain.Entities;

namespace MediatorAuthService.Application.Common.Abstractions;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);

    GeneratedRefreshToken CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);

public sealed record GeneratedRefreshToken(string Token, string TokenHash, DateTime ExpiresAt);
