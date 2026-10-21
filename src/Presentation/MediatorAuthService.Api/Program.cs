using MediatorAuthService.Api.Authorization;
using MediatorAuthService.Api.ExceptionHandling;
using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Api.Middlewares;
using MediatorAuthService.Api.Services;
using MediatorAuthService.Application;
using MediatorAuthService.Application.Common.Abstractions;
using MediatorAuthService.Infrastructure;
using MediatorAuthService.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddElasticSearchLogging();

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Model binding errors (e.g. malformed JSON) are returned in the same envelope as other errors.
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
        ApiResponseWriter.CreateError(
            context.HttpContext,
            (int)HttpStatusCode.BadRequest,
            context.ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage)));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication();
builder.Services.AddPermissionAuthorization();
builder.Services.AddAuthRateLimiting(builder.Configuration);
builder.Services.AddConfiguredCors();
builder.Services.AddApiVersion();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(tags: ["ready"]);

builder.Services.AddObservability(builder.Configuration);

WebApplication app = builder.Build();

// Request logging wraps the exception handler so that it records the final status code, not 500 for handled exceptions.
app.UseSerilogRequestLoggingWithEnrichment();

app.UseExceptionHandler();

if (app.Configuration.GetValue("HttpsRedirection:Enabled", true))
{
    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.AddSwaggerUI();
}

app.UseCors(CorsExtension.PolicyName);

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }).AllowAnonymous();

try
{
    await app.InitializeDatabaseAsync();

    Log.Information("MediatorAuthService başlatılıyor. Ortam: {Environment}", builder.Environment.EnvironmentName);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex is not HostAbortedException)
{
    Log.Fatal(ex, "MediatorAuthService başlatılamadı!");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Entry point; exposed so that integration tests can host the application.
/// </summary>
public partial class Program;
