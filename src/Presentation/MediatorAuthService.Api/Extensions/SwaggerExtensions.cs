using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace MediatorAuthService.Api.Extensions;

/// <summary>
/// Customizes Swagger-related settings.
/// </summary>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider = provider;

    /// <summary>
    /// Configure each API discovered for Swagger Documentation
    /// </summary>
    /// <param name="options"></param>
    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (var description in _provider.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));

        // add jwt security
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security",
        });

        // add jwt bearer security
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Endpoint descriptions
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    }

    /// <summary>
    /// Configure Swagger Options. Inherited from the Interface
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    public void Configure(string name, SwaggerGenOptions options) => Configure(options);

    /// <summary>
    /// Create information about the version of the API
    /// </summary>
    /// <param name="desc"></param>
    /// <returns>Information about the API</returns>
    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription desc)
    {
        OpenApiInfo info = new()
        {
            Title = "Mediator Auth Api",
            Version = desc.ApiVersion.ToString()
        };

        if (desc.IsDeprecated)
            info.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";

        return info;
    }
}

/// <summary>
/// Add SwaggerUI Extension
/// </summary>
public static class AddSwaggerUIExtension
{
    /// <summary>
    /// Add SwaggerUI
    /// </summary>
    public static IApplicationBuilder AddSwaggerUI(this IApplicationBuilder app, WebApplication webApp)
    {
        return app.UseSwaggerUI(c =>
        {
            IApiVersionDescriptionProvider apiVersionDescriptionProvider = webApp.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        });
    }
}