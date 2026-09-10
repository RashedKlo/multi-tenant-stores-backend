// Application/Features/Customers/DTOs/CustomerDto.cs
using Domain.Entities;

namespace Application.Customers.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsEmailVerified,
    bool HasPassword,
    bool HasGoogleAccount,
    DateTime CreatedAt)
{
    public static CustomerDto FromEntity(Customer c) => new(
        c.Id,
        c.FirstName,
        c.LastName,
        c.Email,
        c.IsEmailVerified,
        HasPassword: c.PasswordHash is not null,
        HasGoogleAccount: c.GoogleId is not null,
        c.CreatedAt);
}