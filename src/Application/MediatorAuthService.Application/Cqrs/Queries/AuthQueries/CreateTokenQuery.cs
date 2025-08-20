using MediatorAuthService.Application.Dtos.AuthDtos;
using MediatorAuthService.Application.Wrappers;
using MediatR;

namespace MediatorAuthService.Application.Cqrs.Queries.AuthQueries;

public class CreateTokenQuery : IRequest<ApiResponse<TokenDto>>
{
	public required string Email { get; set; }

	public required string Password { get; set; }
}