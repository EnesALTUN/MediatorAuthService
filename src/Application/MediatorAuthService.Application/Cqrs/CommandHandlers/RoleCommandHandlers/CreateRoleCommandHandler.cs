using MediatorAuthService.Application.Common.Authorization;
using MediatorAuthService.Application.Common.Persistence;
using MediatorAuthService.Application.Cqrs.Commands.RoleCommands;
using MediatorAuthService.Application.Dtos.RoleDtos;
using MediatorAuthService.Application.Exceptions;
using MediatorAuthService.Application.Wrappers;
using MediatorAuthService.Domain.Entities;
using MediatR;
using System.Net;

namespace MediatorAuthService.Application.Cqrs.CommandHandlers.RoleCommandHandlers;

/// <summary>
/// Creates a role. The caller can only include permissions they hold themselves.
/// </summary>
public class CreateRoleCommandHandler(IUnitOfWork _unitOfWork, IAccessGuard _accessGuard) : IRequestHandler<CreateRoleCommand, ApiResponse<RoleDto>>
{
    public async Task<ApiResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        List<string> permissions = [.. request.Permissions.Distinct()];

        await _accessGuard.EnsureCanGrantAsync(permissions, cancellationToken);

        IGenericRepository<Role> repository = _unitOfWork.GetRepository<Role>();
        string name = request.Name.Trim();

        if (await repository.AnyAsync(role => role.Name == name, cancellationToken))
            throw new ConflictException("A role with the same name already exists.");

        Role role = new()
        {
            Name = name,
            Description = request.Description?.Trim(),
            Permissions = [.. permissions.Select(permission => new RolePermission { Permission = permission })]
        };

        await repository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        RoleDto createdRole = await repository.GetByIdAsync(role.Id, RoleDto.Projection, cancellationToken)
            ?? throw new NotFoundException("Role is not found.");

        return new ApiResponse<RoleDto>
        {
            Data = createdRole.WithEffectivePermissions(),
            IsSuccessful = true,
            StatusCode = (int)HttpStatusCode.Created,
            TotalItemCount = 1
        };
    }
}
