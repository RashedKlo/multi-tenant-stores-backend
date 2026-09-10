// Application/Auth/Commands/ResendVerification/ResendVerificationCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.ResendVerification;

public sealed record ResendVerificationCommand(string Email) : IRequest<Result>;