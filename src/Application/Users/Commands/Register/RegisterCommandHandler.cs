using Domain.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Email already exists");

        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            request.Role,
            request.TenantId);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _jwtService.GenerateToken(user);
    }
}
