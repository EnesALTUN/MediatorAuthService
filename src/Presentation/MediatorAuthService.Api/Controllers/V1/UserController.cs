using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.ResponseDtos;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediatorAuthService.Api.Controllers.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
            ApiResponse<UserDto> response = await _mediator.Send(new GetUserByIdQuery(id));

            return ActionResultInstance(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUser([FromQuery] GetUserAllQuery request)
        {
            ApiResponse<List<UserDto>> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            ApiResponse<UserDto> response = await _mediator.Send(new GetUserByEmailQuery(email));

            return ActionResultInstance(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(CreateUserCommand user)
        {
            ApiResponse<UserDto> response = await _mediator.Send(user);

            return ActionResultInstance(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UpdateUserCommand user)
        {
            ApiResponse<UserDto> response = await _mediator.Send(user);

            return ActionResultInstance(response);
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            ApiResponse<NoDataDto> response = await _mediator.Send(new DeleteUserCommand(id));

            return ActionResultInstance(response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordUserCommand request)
        {
            ApiResponse<NoDataDto> response = await _mediator.Send(request);

            return ActionResultInstance(response);
        }
    }
}