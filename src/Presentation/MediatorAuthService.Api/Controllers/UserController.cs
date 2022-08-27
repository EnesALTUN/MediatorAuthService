﻿using MediatorAuthService.Api.Controllers.Base;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Domain.Core.Pagination;
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

            return ActionResultInstance<UserDto>(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUser([FromQuery] PaginationParams paginationParams)
        {
            var response = await _mediator.Send(new GetUserAllQuery(paginationParams));

            return ActionResultInstance<List<UserDto>>(response);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var response = await _mediator.Send(new GetUserByEmailQuery(email));

            return ActionResultInstance<UserDto>(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(CreateUserCommand user)
        {
            var response = await _mediator.Send(user);

            return ActionResultInstance<UserDto>(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UpdateUserCommand user)
        {
            var response = await _mediator.Send(user);

            return ActionResultInstance<UserDto>(response);
        }
    }
}