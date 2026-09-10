// Application/Auth/Commands/ResetPassword/ResetPasswordCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword) : IRequest<Result>;