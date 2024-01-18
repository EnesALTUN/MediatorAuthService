using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Extensions;

/// <summary>
/// Add ApiVersion Extension
/// </summary>
public static class ApiVersioningExtension
{
    /// <summary>
    /// Add ApiVersion
    /// </summary>
    public static IServiceCollection AddApiVersion(this IServiceCollection services)
    {
        services.AddVersionedApiExplorer(o =>
        {
            o.GroupNameFormat = "'v'VVV";
            o.SubstituteApiVersionInUrl = true;
        }).AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
        });

        return services;
    }
}