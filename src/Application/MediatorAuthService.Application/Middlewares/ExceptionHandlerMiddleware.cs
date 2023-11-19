﻿using FluentValidation;
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
        try
        {
            await _next.Invoke(httpContext);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex.Message);

            HttpResponse response = httpContext.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.BadRequest;

            List<string> errors = ex.Errors.Any()
                ? ex.Errors.Select(x => x.ToString()).ToList()
                : [ex.Message];

            string result = JsonSerializer.Serialize(new ApiResponse<string>()
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

            HttpResponse response = httpContext.Response;
            response.ContentType = "application/json";
            response.StatusCode = Convert.ToInt16(ex.HttpStatusCode ?? HttpStatusCode.BadRequest);

            string result = JsonSerializer.Serialize(new ApiResponse<string>()
            {
                Errors = [ex.Message],
                IsSuccessful = false,
                StatusCode = response.StatusCode,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            HttpResponse response = httpContext.Response;
            response.ContentType = "application/json";

            string result = JsonSerializer.Serialize(new ApiResponse<string>()
            {
                Errors = ["Sorry, you do not have the necessary permissions to take the relevant action."],
                IsSuccessful = false,
                StatusCode = (int)HttpStatusCode.Forbidden
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await response.WriteAsync(result);
        }
    }
}