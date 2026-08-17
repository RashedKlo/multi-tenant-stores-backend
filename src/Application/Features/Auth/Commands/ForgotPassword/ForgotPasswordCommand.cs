using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
