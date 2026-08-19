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

        private readonly List<CartItem> _cartItems = new();
        public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();

        private Cart() { }

        public static Result<Cart> CreateForCustomer(Guid customerId, Guid storeId)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");

            if (errors.Count > 0)
                return Result<Cart>.Failure(errors);

            return Result<Cart>.Success(new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                GuestSessionId = null,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public static Result<Cart> CreateForGuest(Guid guestSessionId, Guid storeId)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(guestSessionId, errors, "GuestSessionId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");

            if (errors.Count > 0)
                return Result<Cart>.Failure(errors);

            return Result<Cart>.Success(new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = null,
                GuestSessionId = guestSessionId,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public Result AddItem(Guid productId, int quantity, string? notes, IEnumerable<Guid>? optionIds = null)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(productId, errors, "ProductId");
            DomainValidation.EnsurePositive(quantity, errors, "Quantity");

            if (errors.Count > 0)
                return Result.Failure(errors);

            var existing = _cartItems.FirstOrDefault(i =>
                i.ProductId == productId &&
                OptionsMatch(i, optionIds));

            if (existing is not null)
            {
                var result = existing.IncreaseQuantity(quantity);
                if (result.IsFailure) return result;
                Touch();
                return Result.Success();
            }

            var itemResult = CartItem.Create(Id, productId, quantity, notes);
            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Errors);

            var item = itemResult.Value!;

            if (optionIds is not null)
            {
                foreach (var optionId in optionIds.Distinct())
                {
                    var optResult = CartItemOption.Create(item.Id, optionId);
                    if (optResult.IsFailure)
                        return Result.Failure(optResult.Errors);

                    item.AddOption(optResult.Value!);
                }
            }

            _cartItems.Add(item);
            Touch();
            return Result.Success();
        }

        public Result UpdateItemQuantity(Guid cartItemId, int quantity)
        {
            var item = _cartItems.FirstOrDefault(i => i.Id == cartItemId);
            if (item is null)
                return Result.Failure(new Error("CartItem.NotFound", "Cart item not found."));

            var result = item.Update(quantity);
            if (result.IsFailure) return result;

            Touch();
            return Result.Success();
        }

        public Result RemoveItem(Guid cartItemId)
        {
            var item = _cartItems.FirstOrDefault(i => i.Id == cartItemId);
            if (item is null)
                return Result.Failure(new Error("CartItem.NotFound", "Cart item not found."));

            _cartItems.Remove(item);
            Touch();
            return Result.Success();
        }

        public Result ClearItems()
        {
            _cartItems.Clear();
            Touch();
            return Result.Success();
        }

        public void Touch() => UpdatedAt = DateTime.UtcNow;

        private static bool OptionsMatch(CartItem item, IEnumerable<Guid>? optionIds)
        {
            var existing = item.CartItemOptions.Select(o => o.OptionId).OrderBy(x => x);
            var incoming = (optionIds ?? Enumerable.Empty<Guid>()).OrderBy(x => x);
            return existing.SequenceEqual(incoming);
        }
    }
}