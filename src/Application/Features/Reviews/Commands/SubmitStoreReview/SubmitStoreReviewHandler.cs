using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Reviews.Commands.SubmitStoreReview;

public sealed class SubmitStoreReviewHandler(
    IStoreReviewRepository reviews,
    IOrderRepository orders,
    ICurrentUserService currentUser)
    : IRequestHandler<SubmitStoreReviewCommand, Result>
{
    public async Task<Result> Handle(
        SubmitStoreReviewCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != customerId)
            return Result.Failure(Error.NotFound("Order.NotFound", "Order was not found."));

        if (order.StoreId != request.StoreId)
            return Result.Failure(Error.NotFound("StoreReview.OrderStoreMismatch", "Order does not belong to this store."));

        if (order.Status != OrderStatus.Delivered)
            return Result.Failure(Error.Validation(
                "StoreReview.OrderNotDelivered", "Only delivered orders can be reviewed."));

        if (await reviews.ExistsByOrderIdAsync(request.OrderId, customerId, cancellationToken))
            return Result.Failure(Error.Conflict(
                "StoreReview.AlreadyExists", "This order has already been reviewed."));

        var createResult = StoreReview.Create(
            request.StoreId,
            customerId,
            request.OrderId,
            request.Rating,
            request.Comment);

        if (createResult.IsFailure)
            return Result.Failure(createResult.Errors);

        await reviews.AddAsync(createResult.Value!, cancellationToken);
        await reviews.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}