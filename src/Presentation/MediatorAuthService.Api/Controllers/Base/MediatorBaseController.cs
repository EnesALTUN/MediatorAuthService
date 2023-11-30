using MediatorAuthService.Application.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MediatorAuthService.Api.Controllers.Base;

/// <summary>
/// Base Controller Class
/// </summary>
public class MediatorBaseController : ControllerBase
{
    /// <summary>
    /// It replaces the return status code with the status code from the service.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="response"></param>
    /// <returns></returns>
    public IActionResult ActionResultInstance<TData>(ApiResponse<TData> response) where TData : class
    {
        List<int> allowedHttpStatusReturnCodes = [
            (int)HttpStatusCode.OK,
            (int)HttpStatusCode.Created,
            (int)HttpStatusCode.NoContent,
            (int)HttpStatusCode.BadRequest,
            (int)HttpStatusCode.NotFound
        ];

        return new ObjectResult(response)
        {
            StatusCode = allowedHttpStatusReturnCodes.Exists(allowedHttpStatusReturnCode => allowedHttpStatusReturnCode.Equals(response.StatusCode))
                ? response.StatusCode
                : (int)HttpStatusCode.Forbidden
        };
    }
}