using Application.Users.DTOs;
using MediatR;

namespace Application.Users.Queries.GetAllUsers;

public record GetAllUsersQuery() : IRequest<List<UserDto>>;
