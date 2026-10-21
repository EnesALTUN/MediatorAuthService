using FluentValidation;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Options;
using MediatorAuthService.Application.PipelineBehaviours;
using MediatorAuthService.Application.Services;
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

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.Configure<LockoutOptions>(configuration.GetSection(LockoutOptions.SectionName));

        services.AddScoped<IAccessGuard, AccessGuard>();
        services.AddScoped<IAuthSessionService, AuthSessionService>();

        return services;
    }
}
