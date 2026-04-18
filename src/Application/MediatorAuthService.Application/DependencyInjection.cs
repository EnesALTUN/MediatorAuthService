using FluentValidation;
using MediatorAuthService.Application.Dtos.ConfigurationDtos;
using MediatorAuthService.Application.Extensions;
using MediatorAuthService.Application.PipelineBehaviours;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MediatorAuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.LicenseKey = configuration["MediatR:LicenseKey"] ?? string.Empty;
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.Configure<MediatorTokenOptions>(configuration.GetSection("JwtTokenOption"));

        MediatorTokenOptions? tokenOption = configuration.GetSection("JwtTokenOption").Get<MediatorTokenOptions>() ?? throw new InvalidOperationException("JwtTokenOption section is missing or invalid in configuration.");
        
        services.AddMediatorJwtAuth(tokenOption);

        return services;
    }
}