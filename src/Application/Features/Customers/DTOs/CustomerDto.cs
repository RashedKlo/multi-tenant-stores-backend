namespace Application.Customers.DTOs;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsEmailVerified,
    bool IsActive,
    DateTime CreatedAt)
{
    public static CustomerDto FromEntity(Domain.Entities.Customer c) => new(
        c.Id,
        c.FirstName,
        c.LastName,
        c.Email,
        c.IsEmailVerified,
        c.IsActive,
        c.CreatedAt);
}
