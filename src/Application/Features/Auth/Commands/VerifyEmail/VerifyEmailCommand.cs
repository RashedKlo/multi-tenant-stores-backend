// Application/Auth/Commands/VerifyEmail/VerifyEmailCommand.cs
using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(
    string Email,
    string Code) : IRequest<Result<AuthTokensDto>>;