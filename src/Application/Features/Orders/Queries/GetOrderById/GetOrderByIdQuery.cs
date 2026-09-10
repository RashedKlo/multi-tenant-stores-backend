using Application.Features.Orders.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDetailDto>>;
