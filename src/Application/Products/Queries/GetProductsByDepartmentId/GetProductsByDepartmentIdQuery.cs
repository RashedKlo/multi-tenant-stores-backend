using Application.Common.DTOs;
using MediatR;

namespace Application.Products.Queries.GetProductsByDepartmentId;

public record GetProductsByDepartmentIdQuery(Guid DepartmentId) : IRequest<List<ProductDto>>;
