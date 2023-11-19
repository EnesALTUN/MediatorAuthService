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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));

        services.AddAutoMapper(assembly);

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        services.Configure<MediatorTokenOptions>(configuration.GetSection("JwtTokenOption"));

        MediatorTokenOptions tokenOption = configuration.GetSection("JwtTokenOption").Get<MediatorTokenOptions>();
        services.AddMediatorJwtAuth(tokenOption);

        return services;
    }
}