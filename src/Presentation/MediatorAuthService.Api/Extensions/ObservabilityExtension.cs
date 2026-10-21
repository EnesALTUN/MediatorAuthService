using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MediatorAuthService.Api.Extensions;

internal static class ObservabilityExtension
{
    /// <summary>
    /// Exports traces and metrics over OTLP when OpenTelemetry:OtlpEndpoint is configured.
    /// </summary>
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        string? otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        if (string.IsNullOrWhiteSpace(otlpEndpoint))
            return services;

        Uri endpoint = new(otlpEndpoint);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("MediatorAuthService"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = endpoint))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = endpoint));

        return services;
    }
}
