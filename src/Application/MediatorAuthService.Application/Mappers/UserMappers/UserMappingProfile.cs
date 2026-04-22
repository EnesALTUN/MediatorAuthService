using AutoMapper;
using MediatorAuthService.Application.Cqrs.Commands.UserCommands;
using MediatorAuthService.Application.Cqrs.Queries.UserQueries;
using MediatorAuthService.Application.Dtos.UserDtos;
using MediatorAuthService.Domain.Entities;

namespace MediatorAuthService.Application.Mappers.UserMappers;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<CreateUserCommand, User>();
        CreateMap<UpdateUserCommand, User>();
        CreateMap<GetUserByIdQuery, User>();
    }
}