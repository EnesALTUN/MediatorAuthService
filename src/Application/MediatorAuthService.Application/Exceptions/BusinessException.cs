using System.Net;

namespace MediatorAuthService.Application.Exceptions;

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