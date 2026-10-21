using System.Net;

namespace MediatorAuthService.Application.Exceptions;

/// <summary>
/// An expected error whose message is safe to return to the client. Defaults to 400 Bad Request.
/// </summary>
public class BusinessException : Exception
{
    public HttpStatusCode? HttpStatusCode { get; private set; }

    public BusinessException(string? message) : base(message)
    {
    }

    public BusinessException(string? message, HttpStatusCode? httpStatusCode) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }

    public BusinessException(string? message, Exception? innerException, HttpStatusCode? httpStatusCode) : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}

public class NotFoundException(string message) : BusinessException(message, System.Net.HttpStatusCode.NotFound);

public class UnauthorizedException(string message) : BusinessException(message, System.Net.HttpStatusCode.Unauthorized);

public class ForbiddenException(string message) : BusinessException(message, System.Net.HttpStatusCode.Forbidden);

public class ConflictException(string message, Exception? innerException = null) : BusinessException(message, innerException, System.Net.HttpStatusCode.Conflict);
