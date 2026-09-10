using Domain.Common;

namespace Domain.Entities;

public sealed class FavoriteProduct
{
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime CreatedAt { get; private set; }
 
    public Product Product { get; private set; } = null!;
    private FavoriteProduct()
    {
    }

    public static Result<FavoriteProduct> Create(Guid customerId, Guid productId)
    {
        var favorite = new FavoriteProduct
        {
            CreatedAt = DateTime.UtcNow
        };

        return favorite
            .SetCustomerId(customerId)
            .Bind(() => favorite.SetProductId(productId))
            .Bind(() => Result<FavoriteProduct>.Success(favorite));
    }

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation("FavoriteProduct.CustomerId.Required", "CustomerId is required."));

        CustomerId = customerId;
        return Result.Success();
    }

    private Result SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            return Result.Failure(Error.Validation("FavoriteProduct.ProductId.Required", "ProductId is required."));

        ProductId = productId;
        return Result.Success();
    }
}