using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace MediatorAuthService.Api.Extensions;

internal static class CorsExtension
{
    public const string PolicyName = "ConfiguredOrigins";

    /// <summary>
    /// Allows only the origins listed under Cors:AllowedOrigins. With an empty list no cross-origin request is allowed.
    /// </summary>
    public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
    {
        services.AddCors();

        services.AddOptions<CorsOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

                options.AddPolicy(PolicyName, policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

        return services;
    }
}
