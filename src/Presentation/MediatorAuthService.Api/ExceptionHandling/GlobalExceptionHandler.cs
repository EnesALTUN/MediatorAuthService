using FluentValidation;
using MediatorAuthService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace MediatorAuthService.Api.ExceptionHandling;

/// <summary>
/// Converts exceptions into ApiResponse error bodies with a matching HTTP status code.
/// Only messages of expected exceptions are returned; unexpected errors get a generic message and a trace ID.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("The request was cancelled by the client.");
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        (int statusCode, string[] errors) = exception switch
        {
            ValidationException ex => ((int)HttpStatusCode.BadRequest, ex.Errors.Any() ? ex.Errors.Select(x => x.ErrorMessage).ToArray() : [ex.Message]),
            BusinessException ex => ((int)(ex.HttpStatusCode ?? HttpStatusCode.BadRequest), [ex.Message]),
            BadHttpRequestException ex => (ex.StatusCode, ["The request is invalid."]),
            _ => ((int)HttpStatusCode.InternalServerError, ["An unexpected error occurred. Please contact support with the trace ID."])
        };

        if (statusCode >= (int)HttpStatusCode.InternalServerError)
            logger.LogError(exception, "An unhandled exception occurred.");
        else
            logger.LogWarning("Request failed with {StatusCode}: {Errors}", statusCode, errors);

        await ApiResponseWriter.WriteAsync(httpContext, statusCode, errors);

        return true;
    }
}
