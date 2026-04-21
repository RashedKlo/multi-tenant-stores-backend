using Application.Users.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Users.Queries.GetUserByEmail;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return null;

        return new UserDto(
            user.Id, user.FirstName, user.LastName, user.Email,
            user.Role, user.TenantId, user.IsActive,
            user.CreatedAt, user.UpdatedAt
        );
    }
}
