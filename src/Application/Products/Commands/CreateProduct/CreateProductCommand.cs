using Application.Common.DTOs;
using MediatR;

namespace Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid DepartmentId,
    string Title,
    string? Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    string? SKU
) : IRequest<ProductDto>;
