namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction over password hashing (BCrypt / ASP.NET Identity / etc.).
/// Implemented in Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
