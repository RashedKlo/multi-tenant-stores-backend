using Application.Users.DTOs;
using MediatR;

namespace Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
