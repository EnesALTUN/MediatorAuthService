using MediatorAuthService.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MediatorAuthService.Application.PipelineBehaviours;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IHttpContextAccessor httpContextAccessor) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string? requestName = typeof(TRequest).Name;
        string? userId = httpContextAccessor.HttpContext?.User?.Identity?.Name;
        string? correlationId = httpContextAccessor.HttpContext?.TraceIdentifier;
        Stopwatch? sw = Stopwatch.StartNew();

        object? requestData = request is ISensitiveRequest
            ? "[REDACTED]"
            : LogSanitizer.Sanitize(request);

        _logger.LogInformation("MediatR Request Started {@Log}",
            new
            {
                RequestName = requestName,
                UserId = userId,
                CorrelationId = correlationId,
                Request = requestData
            }
        );

        TResponse? response = await next(cancellationToken);

        sw.Stop();

        _logger.LogInformation("MediatR Request Completed {@Log}",
            new
            {
                RequestName = requestName,
                UserId = userId,
                CorrelationId = correlationId,
                ElapsedMs = sw.ElapsedMilliseconds
            }
        );

        return response;
    }
}