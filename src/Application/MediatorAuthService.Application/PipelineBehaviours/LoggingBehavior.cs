using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MediatorAuthService.Application.PipelineBehaviours;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUserService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Guid? userId = currentUserService.UserId;
        string? correlationId = Activity.Current?.TraceId.ToString();
        Stopwatch sw = Stopwatch.StartNew();

        _logger.LogInformation("MediatR Request Started {@Log}",
            new
            {
                RequestName = requestName,
                UserId = userId,
                CorrelationId = correlationId,
                Request = LogSanitizer.Sanitize(request)
            }
        );

        TResponse response = await next(cancellationToken);

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
