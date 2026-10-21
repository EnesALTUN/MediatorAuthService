using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Wrappers;
using System.Diagnostics;
using System.Text.Json;

namespace MediatorAuthService.Api.ExceptionHandling;

/// <summary>
/// Writes error responses in the same ApiResponse envelope that controllers return.
/// </summary>
internal static class ApiResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string GetTraceId(HttpContext httpContext) => Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

    public static ApiResponse<NoData> CreateError(HttpContext httpContext, int statusCode, IEnumerable<string> errors) => new()
    {
        Errors = [.. errors],
        IsSuccessful = false,
        StatusCode = statusCode,
        TraceId = GetTraceId(httpContext)
    };

    public static async Task WriteAsync(HttpContext httpContext, int statusCode, params string[] errors)
    {
        if (httpContext.Response.HasStarted)
            return;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(CreateError(httpContext, statusCode, errors), JsonOptions), httpContext.RequestAborted);
    }
}
