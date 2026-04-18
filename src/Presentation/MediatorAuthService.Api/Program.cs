using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Api.Middlewares;
using MediatorAuthService.Application;
using MediatorAuthService.Application.Middlewares;
using MediatorAuthService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddElasticSearchLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiVersion();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

WebApplication app = builder.Build();

app.UseSerilogRequestLoggingWithEnrichment();

app.UseCors(cors => cors.AllowAnyHeader()
                        .AllowAnyOrigin()
);

app.ApplyMigration();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.AddSwaggerUI(app);
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
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