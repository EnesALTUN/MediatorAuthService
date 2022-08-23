using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Domain.Core.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _mediator.Send(new GetUserByIdQuery(id));

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUser([FromQuery] PaginationParams paginationParams)
        {
            var response = await _mediator.Send(new GetUserAllQuery(paginationParams));

            return Ok(response);
        }
    }
}