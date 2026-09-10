// Application/Auth/Commands/Logout/LogoutCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;