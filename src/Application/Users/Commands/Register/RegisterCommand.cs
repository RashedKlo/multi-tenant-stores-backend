using Application.Users.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserRole Role,
    Guid? TenantId) : IRequest<string>;
