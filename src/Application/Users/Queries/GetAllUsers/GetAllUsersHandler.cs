using Application.Users.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Users.Queries.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var dtos = users.Select(u => new UserDto(
            u.Id, u.FirstName, u.LastName, u.Email,
            u.Role, u.TenantId, u.IsActive,
            u.CreatedAt, u.UpdatedAt
        )).ToList();

        return dtos;
    }
}
