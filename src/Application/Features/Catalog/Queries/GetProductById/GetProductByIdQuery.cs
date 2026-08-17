using Application.Catalog.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetProductById;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDetailDto>>;
