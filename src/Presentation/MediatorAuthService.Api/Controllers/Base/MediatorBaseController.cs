using MediatorAuthService.Application.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MediatorAuthService.Api.Controllers.Base;

public class MediatorBaseController : ControllerBase
{
    public IActionResult ActionResultInstance<TData>(ApiResponse<TData> response) where TData : class
    {
        List<int> allowedHttpStatusReturnCodes = new() {
            (int)HttpStatusCode.OK,
            (int)HttpStatusCode.Created,
            (int)HttpStatusCode.NoContent,
            (int)HttpStatusCode.BadRequest,
            (int)HttpStatusCode.NotFound
        };

        return new ObjectResult(response)
        {
            StatusCode = allowedHttpStatusReturnCodes.Any(allowedHttpStatusReturnCode => allowedHttpStatusReturnCode.Equals(response.StatusCode))
                ? response.StatusCode
                : (int)HttpStatusCode.Forbidden
        };
    }
}