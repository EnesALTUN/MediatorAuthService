using MediatorAuthService.Api.Extensions;
using MediatorAuthService.Application;
using MediatorAuthService.Application.Middlewares;
using MediatorAuthService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .Build();

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

app.Run();