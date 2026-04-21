using Domain.Entities;

namespace Application.Users.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid? TenantId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
