using Domain.Common;

namespace Domain.Entities;

public sealed class StoreReview
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public short Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Store Store { get; private set; } = null!;
    public Customer Customer { get; private set; } = null!;
    public Order Order { get; private set; } = null!;

    private StoreReview()
    {
    }

    public static Result<StoreReview> Create(
        Guid storeId,
        Guid customerId,
        Guid orderId,
        short rating,
        string? comment)
    {
        var review = new StoreReview
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        return review
            .SetStoreId(storeId)
            .Bind(() => review.SetCustomerId(customerId))
            .Bind(() => review.SetOrderId(orderId))
            .Bind(() => review.SetRating(rating))
            .Bind(() => review.SetComment(comment))
            .Bind(() => Result<StoreReview>.Success(review));
    }

    private Result SetStoreId(Guid storeId)
    {
        if (storeId == Guid.Empty)
            return Result.Failure(Error.Validation("StoreReview.StoreId.Required", "StoreId is required."));

        StoreId = storeId;
        return Result.Success();
    }

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation("StoreReview.CustomerId.Required", "CustomerId is required."));

        CustomerId = customerId;
        return Result.Success();
    }

    private Result SetOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return Result.Failure(Error.Validation("StoreReview.OrderId.Required", "OrderId is required."));

        OrderId = orderId;
        return Result.Success();
    }

    private Result SetRating(short rating)
    {
        if (rating is < 1 or > 5)
            return Result.Failure(Error.Validation("StoreReview.Rating.Invalid", "Rating must be between 1 and 5."));

        Rating = rating;
        return Result.Success();
    }

    private Result SetComment(string? comment)
    {
        if (comment is not null && comment.Length > 1000)
            return Result.Failure(Error.Validation("StoreReview.Comment.TooLong", "Comment cannot exceed 1000 characters."));

        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        return Result.Success();
    }
}