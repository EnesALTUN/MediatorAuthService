using FluentValidation;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace MediatorAuthService.Application.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    public async Task Invoke(HttpContext httpContext)
    {
        JsonSerializerOptions jsonOption = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string responseContentType = "application/json";

        try
        {
            await _next.Invoke(httpContext);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex.Message);

            HttpResponse response = httpContext.Response;
            response.ContentType = responseContentType;
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            List<string> errors = ex.Errors.Any()
                ? ex.Errors.Select(x => x.ToString()).ToList()
                : [ex.Message];

            string result = JsonSerializer.Serialize(new ApiResponse<INoData>()
            {
                Errors = errors,
                IsSuccessful = false,
                StatusCode = response.StatusCode
            }, jsonOption);

            await response.WriteAsync(result);
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex.Message);

            HttpResponse response = httpContext.Response;
            response.ContentType = responseContentType;
            response.StatusCode = Convert.ToInt16(ex.HttpStatusCode ?? HttpStatusCode.BadRequest);

            string result = JsonSerializer.Serialize(new ApiResponse<INoData>()
            {
                Errors = [ex.Message],
                IsSuccessful = false,
                StatusCode = response.StatusCode,
            }, jsonOption);

            await response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            HttpResponse response = httpContext.Response;
            response.ContentType = responseContentType;

            string result = JsonSerializer.Serialize(new ApiResponse<INoData>()
            {
                Errors = ["Sorry, you do not have the necessary permissions to take the relevant action."],
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.Forbidden
            }, jsonOption);

            await response.WriteAsync(result);
        }
    }
}