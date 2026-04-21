using Domain.Entities;

namespace Domain.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
