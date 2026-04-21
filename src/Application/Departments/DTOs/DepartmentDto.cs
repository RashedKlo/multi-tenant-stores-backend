namespace Application.Common.DTOs;

public record DepartmentDto(
    Guid Id,
    Guid StoreId,
    string Name,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
