using MediatorAuthService.Api.ExceptionHandling;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace MediatorAuthService.Api.Extensions;

internal sealed class AuthRateLimitOptions
{
    public const string SectionName = "RateLimiting:Auth";

    public int PermitLimit { get; set; } = 10;

    public int WindowSeconds { get; set; } = 60;
}

internal static class RateLimitingExtension
{
    /// <summary>
    /// Policy for credential endpoints (login, refresh, logout, change password), partitioned by client IP.
    /// Behind a reverse proxy, list it under ForwardedHeaders so that the real client IP is used (see ForwardedHeadersExtension).
    /// </summary>
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthRateLimitOptions>(configuration.GetSection(AuthRateLimitOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                await ApiResponseWriter.WriteAsync(context.HttpContext, (int)HttpStatusCode.TooManyRequests, "Too many requests. Please try again later.");
            };

            options.AddPolicy(AuthPolicy, httpContext =>
            {
                AuthRateLimitOptions limits = httpContext.RequestServices.GetRequiredService<IOptions<AuthRateLimitOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.PermitLimit,
                        Window = TimeSpan.FromSeconds(limits.WindowSeconds),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
