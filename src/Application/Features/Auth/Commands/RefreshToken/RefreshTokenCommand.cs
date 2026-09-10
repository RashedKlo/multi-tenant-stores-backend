// Application/Auth/Commands/RefreshToken/RefreshTokenCommand.cs
using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokensDto>>;