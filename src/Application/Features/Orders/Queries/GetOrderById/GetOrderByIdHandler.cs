using Application.Common.Interfaces;
using Application.Features.Orders.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdHandler(
    IOrderRepository orders,
    ICurrentUserService user)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<OrderDetailDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var order = await orders.GetByIdForCustomerAsync(
            request.Id,
            user.CustomerId.Value,
            cancellationToken);

        if (order is null)
            return Result<OrderDetailDto>.Failure(
                Error.NotFound("Order.NotFound", "Order not found."));

        return Result<OrderDetailDto>.Success(OrderDetailDto.FromEntity(order));
    }
}
