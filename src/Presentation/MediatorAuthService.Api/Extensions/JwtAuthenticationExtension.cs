using MediatorAuthService.Api.ExceptionHandling;
using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace MediatorAuthService.Api.Extensions;

internal static class JwtAuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configured through options so that the validated JwtOptions are used.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                JwtOptions tokenOption = jwtOptions.Value;

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = tokenOption.Issuer,
                    ValidAudiences = tokenOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOption.SecurityKey)),
                    NameClaimType = CustomClaimTypes.Subject,

                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateSecurityStampAsync,
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
                        await ApiResponseWriter.WriteAsync(context.HttpContext, (int)HttpStatusCode.Unauthorized, "Authentication is required or the access token is invalid.");
                    },
                    OnForbidden = context =>
                        ApiResponseWriter.WriteAsync(context.HttpContext, (int)HttpStatusCode.Forbidden, "You do not have permission to perform this action.")
                };
            });

        return services;
    }

    /// <summary>
    /// Rejects tokens of deleted or deactivated users and tokens issued before the last password change or session revocation.
    /// </summary>
    private static async Task ValidateSecurityStampAsync(TokenValidatedContext context)
    {
        Guid? userId = context.Principal?.GetUserId();
        string? securityStamp = context.Principal?.GetSecurityStamp();

        if (userId is null || securityStamp is null)
        {
            context.Fail("The access token is missing required claims.");
            return;
        }

        IUserAccessService userAccessService = context.HttpContext.RequestServices.GetRequiredService<IUserAccessService>();
        UserAccessSnapshot? access = await userAccessService.GetAsync(userId.Value, context.HttpContext.RequestAborted);

        if (access is not { IsActive: true } || access.SecurityStamp.ToString() != securityStamp)
            context.Fail("The access token is no longer valid.");
    }
}
