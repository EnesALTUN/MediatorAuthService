using Serilog;

namespace MediatorAuthService.Api.Middlewares;

/// <summary>
/// Contains extension methods for configuring Serilog request logging middleware
/// with enriched contextual information.
/// </summary>
public static class SerilogRequestLoggingMiddleware
{
    /// <summary>
    /// Adds Serilog request logging to the application pipeline with enriched context,
    /// including correlation ID, user ID, request host, and user agent metadata.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance for method chaining.</returns>
    public static IApplicationBuilder UseSerilogRequestLoggingWithEnrichment(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} {StatusCode} {Elapsed:0.0000}ms";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                diagnosticContext.Set("UserId", httpContext.User.Identity?.Name);
            };
        });

        return app;
    }
}