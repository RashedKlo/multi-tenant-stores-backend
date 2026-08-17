using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokensDto>>;
