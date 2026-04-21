using MediatR;

namespace Application.Users.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<string>;
