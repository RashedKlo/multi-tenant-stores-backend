// Application/Auth/Commands/GoogleLogin/GoogleLoginCommand.cs
using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.GoogleLogin;

public sealed record GoogleLoginCommand(string IdToken) : IRequest<Result<AuthTokensDto>>;