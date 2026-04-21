using Application.Common.DTOs;
using MediatR;

namespace Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Title,
    string? Description,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    string? ImageUrl
) : IRequest<ProductDto?>;
