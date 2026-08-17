using Domain.Common;
using MediatR;

namespace Application.Customers.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Result>;
