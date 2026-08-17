using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(
    string IdToken) : IRequest<Result<AuthTokensDto>>;
