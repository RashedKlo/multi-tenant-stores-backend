using Application.Users.DTOs;
using MediatR;

namespace Application.Users.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;
