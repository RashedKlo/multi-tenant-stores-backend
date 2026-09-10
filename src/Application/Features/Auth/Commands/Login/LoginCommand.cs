// Application/Auth/Commands/Login/LoginCommand.cs
using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthTokensDto>>;