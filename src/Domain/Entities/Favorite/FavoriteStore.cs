using Domain.Common;

namespace Domain.Entities;

public sealed class FavoriteStore
{
    public Guid CustomerId { get; private set; }
    public Guid StoreId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Store Store { get; private set; } = null!;
    private FavoriteStore()
    {
    }

    public static Result<FavoriteStore> Create(Guid customerId, Guid storeId)
    {
        var favorite = new FavoriteStore
        {
            CreatedAt = DateTime.UtcNow
        };

        return favorite
            .SetCustomerId(customerId)
            .Bind(() => favorite.SetStoreId(storeId))
            .Bind(() => Result<FavoriteStore>.Success(favorite));
    }

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation("FavoriteStore.CustomerId.Required", "CustomerId is required."));

        CustomerId = customerId;
        return Result.Success();
    }

    private Result SetStoreId(Guid storeId)
    {
        if (storeId == Guid.Empty)
            return Result.Failure(Error.Validation("FavoriteStore.StoreId.Required", "StoreId is required."));

        StoreId = storeId;
        return Result.Success();
    }
}