// Application/Auth/Commands/Register/RegisterCommand.cs
using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<RegisterResultDto>>;