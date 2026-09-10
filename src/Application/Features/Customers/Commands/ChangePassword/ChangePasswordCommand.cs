// Application/Features/Customers/Commands/ChangePassword/ChangePasswordCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Customers.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Result>;