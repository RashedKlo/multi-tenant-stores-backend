using Application.Users.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid? TenantId,
    bool IsActive) : IRequest<UserDto?>;
