using MediatorAuthService.Application.Common.Extensions;
using MediatorAuthService.Application.Common.Options;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.AuthCommands;
using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Services;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.AuthCommandHandlers;

/// <summary>
/// Verifies the credentials and issues an access token and a refresh token.
/// - Unknown e-mail addresses and wrong passwords return the same error, and an unknown address still pays the hashing cost
///   so that response times do not reveal which addresses are registered.
/// - Consecutive failed attempts lock the account for a configured period.
/// </summary>
public class LoginCommandHandler(IUnitOfWork _unitOfWork, IAuthSessionService _authSessionService, IOptions<LockoutOptions> _lockoutOptions) : IRequestHandler<LoginCommand, ApiResponse<TokenDto>>
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    private static readonly string DummyPasswordHash = HashingManager.HashValue(Guid.NewGuid().ToString());

    public async Task<ApiResponse<TokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        string email = request.Email.NormalizeEmail();

        User? existUser = await _unitOfWork.GetRepository<User>()
            .Where(user => user.Email == email)
            .SingleOrDefaultAsync(cancellationToken);

        if (existUser is null)
        {
            HashingManager.VerifyHashedValue(DummyPasswordHash, request.Password);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        DateTime now = DateTime.UtcNow;

        if (existUser.IsLockedOut(now))
            throw new UnauthorizedException("Too many failed login attempts. Please try again later.");

        if (!HashingManager.VerifyHashedValue(existUser.Password, request.Password))
        {
            await RegisterFailedAttemptAsync(existUser, now, cancellationToken);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (!existUser.IsActive)
            throw new UnauthorizedException("The user account is disabled.");

        existUser.AccessFailedCount = 0;
        existUser.LockoutEnd = null;

        (TokenDto token, _) = await _authSessionService.IssueAsync(existUser, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApiResponse<TokenDto>
        {
            Data = token,
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.OK,
            TotalItemCount = 1
        };
    }

    private async Task RegisterFailedAttemptAsync(User user, DateTime now, CancellationToken cancellationToken)
    {
        LockoutOptions lockoutOptions = _lockoutOptions.Value;

        user.AccessFailedCount++;

        if (user.AccessFailedCount >= lockoutOptions.MaxFailedAccessAttempts)
        {
            user.LockoutEnd = now.AddMinutes(lockoutOptions.LockoutMinutes);
            user.AccessFailedCount = 0;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
