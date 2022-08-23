using MediatorAuthService.Application.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Controllers.Base;

public class MediatorBaseController : ControllerBase
{
    public IActionResult ActionResultInstance<TData>(ApiResponse<TData> response) where TData : class
    {
        return new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };
    }
}