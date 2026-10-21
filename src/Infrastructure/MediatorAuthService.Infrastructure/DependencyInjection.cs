using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Infrastructure.Data.Context;
using MediatorAuthService.Infrastructure.Data.Seed;
using MediatorAuthService.Infrastructure.Security;
using MediatorAuthService.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorAuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            options.UseSqlServer(
                serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Default"),
                mig => mig.MigrationsAssembly("MediatorAuthService.Api")));

        services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !options.Validate().Any(), $"{JwtOptions.SectionName} configuration is invalid. Check Audience, Issuer, SecurityKey (min {JwtOptions.MinimumSecurityKeyBytes} bytes) and expiration values.")
            .ValidateOnStart();

        services.AddOptions<SeedOptions>()
            .Bind(configuration.GetSection(SeedOptions.SectionName));

        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddMemoryCache();
        services.AddSingleton<IUserAccessService, UserAccessService>();

        return services;
    }
}
