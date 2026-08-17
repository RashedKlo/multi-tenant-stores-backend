using Domain.Common;

namespace Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; private set; }

        public Guid? CustomerId { get; private set; }

        public Guid? GuestSessionId { get; private set; }

        public Guid StoreId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }
        public Customer? Customer { get; private set; }
        public GuestSession? GuestSession { get; private set; }
        public Store? Store { get; private set; }
        public ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();

        private Cart()
        {
        }

        public static Result<Cart> CreateForCustomer(Guid customerId, Guid storeId)
        {
            var errors = new List<Error>();

          DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");

            if (errors.Count > 0)
                return Result<Cart>.Failure(errors);

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                GuestSessionId = null,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Cart>.Success(cart);
        }

        public static Result<Cart> CreateForGuest(Guid guestSessionId, Guid storeId)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(guestSessionId, errors, "GuestSessionId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");
            
            if (errors.Count > 0)
                return Result<Cart>.Failure(errors);

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = null,
                GuestSessionId = guestSessionId,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Cart>.Success(cart);
        }

        public void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}