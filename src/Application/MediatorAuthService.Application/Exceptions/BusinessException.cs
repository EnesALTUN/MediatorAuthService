using System.Net;
using System.Runtime.Serialization;

namespace MediatorAuthService.Application.Exceptions;

[Serializable]
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

    protected BusinessException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}