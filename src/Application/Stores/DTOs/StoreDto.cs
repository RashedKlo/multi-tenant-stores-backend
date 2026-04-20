namespace Application.Stores.DTOs;

public record StoreDto
(Guid Id,
 Guid TenantId,
 string Name,
 string? Description,
 string? LogoUrl,
 string? BannerUrl,
 bool IsActive,
 DateTime CreatedAt,
 DateTime? UpdatedAt);
