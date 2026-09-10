// Application/Auth/Commands/ForgotPassword/ForgotPasswordCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;