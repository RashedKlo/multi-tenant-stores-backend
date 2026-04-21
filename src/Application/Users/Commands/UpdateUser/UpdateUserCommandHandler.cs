using Application.Users.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return null;

        user.Update(request.FirstName, request.LastName, request.Email, request.Role, request.TenantId, request.IsActive);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id, user.FirstName, user.LastName, user.Email,
            user.Role, user.TenantId, user.IsActive,
            user.CreatedAt, user.UpdatedAt
        );
    }
}
