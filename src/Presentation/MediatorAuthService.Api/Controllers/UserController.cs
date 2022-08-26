using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Domain.Core.Pagination;
using MediatorAuthService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : MediatorBaseController
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

            return ActionResultInstance<User>(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUser([FromQuery] PaginationParams paginationParams)
        {
            var response = await _mediator.Send(new GetUserAllQuery(paginationParams));

            return ActionResultInstance<List<User>>(response);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var response = await _mediator.Send(new GetUserByEmailQuery(email));

            return ActionResultInstance<User>(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            var response = await _mediator.Send(new CreateUserCommand(user));

            return ActionResultInstance<User>(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(User user)
        {
            var response = await _mediator.Send(new UpdateUserCommand(user));

            return ActionResultInstance<User>(response);
        }
    }
}