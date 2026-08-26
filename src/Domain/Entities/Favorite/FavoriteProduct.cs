using Domain.Common;

namespace Domain.Entities
{
    public class FavoriteProduct
    {
        public Guid CustomerId { get; private set; }

        public Guid ProductId { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public Customer Customer { get; private set; } = null!;
        public Product Product { get; private set; } = null!;

        private FavoriteProduct()
        {
        }

        public static Result<FavoriteProduct> Create(Guid customerId, Guid productId)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");
            DomainValidation.EnsureNotEmptyGuid(productId, errors, "ProductId");
            if (errors.Count > 0)
                return Result<FavoriteProduct>.Failure(errors);

            var favorite = new FavoriteProduct
            {
                CustomerId = customerId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            return Result<FavoriteProduct>.Success(favorite);
        }
    }
}