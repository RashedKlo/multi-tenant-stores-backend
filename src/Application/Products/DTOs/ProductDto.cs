namespace Application.Common.DTOs;

public record ProductDto(
    Guid Id,
    Guid DepartmentId,
    string Title,
    string? Description,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    string? ImageUrl,
    string? SKU,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
