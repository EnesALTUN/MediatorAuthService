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
    /// Returns the response with the status code set by the handler. 204 responses have no body.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="response"></param>
    /// <returns></returns>
    protected IActionResult ActionResultInstance<TData>(ApiResponse<TData> response)
    {
        return response.StatusCode == (int)HttpStatusCode.NoContent
            ? NoContent()
            : StatusCode(response.StatusCode, response);
    }
}
