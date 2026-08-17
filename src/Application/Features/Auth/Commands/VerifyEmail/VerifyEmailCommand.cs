using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Email, string Code) : IRequest<Result<AuthTokensDto>>;
