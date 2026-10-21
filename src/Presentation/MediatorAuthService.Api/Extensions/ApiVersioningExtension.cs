using Asp.Versioning;

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
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
