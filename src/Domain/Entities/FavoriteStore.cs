using Domain.Common;

namespace Domain.Entities
{
    public class FavoriteStore
    {
        public Guid CustomerId { get; private set; }

        public Guid StoreId { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public Customer Customer { get; private set; } = null!;
        public Store Store { get; private set; } = null!;

        private FavoriteStore()
        {
        }

        public static Result<FavoriteStore> Create(Guid customerId, Guid storeId)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");

            if (errors.Count > 0)
                return Result<FavoriteStore>.Failure(errors);

            var favorite = new FavoriteStore
            {
                CustomerId = customerId,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow
            };

            return Result<FavoriteStore>.Success(favorite);
        }
    }
}