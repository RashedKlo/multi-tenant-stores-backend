using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.ResendVerification;

public record ResendVerificationCommand(string Email) : IRequest<Result>;
