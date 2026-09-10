using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Domain.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<OrderSummaryDto>>>;
