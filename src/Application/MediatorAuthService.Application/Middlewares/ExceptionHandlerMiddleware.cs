using FluentValidation;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace MediatorAuthService.Application.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next.Invoke(httpContext);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex.Message);

            var response = httpContext.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            List<string> errors = ex.Errors.Any()
                ? ex.Errors.Select(x => x.ToString()).ToList()
                : new List<string> { ex.Message };

            var result = JsonSerializer.Serialize(new ApiResponse<string>()
            {
                Errors = errors,
                IsSuccessful = false,
                StatusCode = response.StatusCode
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await response.WriteAsync(result);
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex.Message);

            var response = httpContext.Response;
            response.ContentType = "application/json";
            response.StatusCode = Convert.ToInt16(ex.HttpStatusCode ?? HttpStatusCode.BadRequest);

            var result = JsonSerializer.Serialize(new ApiResponse<string>()
            {
                Errors = new List<string> { ex.Message },
                IsSuccessful = false,
                StatusCode = response.StatusCode,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            var response = httpContext.Response;
            response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new ApiResponse<string>()
            {
                Errors = new List<string> { "Sorry, you do not have the necessary permissions to take the relevant action." },
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.Forbidden
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await response.WriteAsync(result);
        }
    }
}