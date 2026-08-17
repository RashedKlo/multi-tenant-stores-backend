using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
