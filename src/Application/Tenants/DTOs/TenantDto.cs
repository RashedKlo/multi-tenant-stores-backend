namespace Application.Tenants.DTOs;

public record TenantDto
(Guid Id,
 string Name,
 string Email,
 bool IsActive,
 DateTime CreatedAt,
 DateTime? UpdatedAt,
 DateTime? DeletedAt);