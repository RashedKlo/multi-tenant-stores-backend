using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Orders.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersHandler(
    IOrderRepository orders,
    ICurrentUserService user)
    : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<PagedResult<OrderSummaryDto>>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 50 ? 20 : request.PageSize;

        var (items, totalCount) = await orders.GetPagedByCustomerAsync(
            user.CustomerId.Value,
            request.Status,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(OrderSummaryDto.FromEntity).ToList();

        return Result<PagedResult<OrderSummaryDto>>.Success(
            PagedResult<OrderSummaryDto>.Create(dtos, page, pageSize, totalCount));
    }
}
