using System.Globalization;
using Domain.Common;

namespace Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; private set; }
        public Guid CartId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public string? Notes { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Cart Cart { get; private set; } = null!;
        public Product Product { get; private set; } = null!;

public ICollection<CartItemOption> CartItemOptions { get; private set; } = new List<CartItemOption>();

        private CartItem() { }

        public static Result<CartItem> Create(Guid cartId, Guid productId, int quantity, string? notes = null)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(cartId, errors, "CartId");
            DomainValidation.EnsureNotEmptyGuid(productId, errors, "ProductId");
            DomainValidation.EnsurePositive(quantity, errors, "Quantity");
            notes = DomainValidation.NormalizeOptional(notes);

            if (errors.Count > 0)
                return Result<CartItem>.Failure(errors);

            return Result<CartItem>.Success(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                Notes = notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public Result Update(int quantity, string? notes = null)
        {
            var errors = new List<Error>();
            DomainValidation.EnsurePositive(quantity, errors, "Quantity");
            notes = DomainValidation.NormalizeOptional(notes);

            if (errors.Count > 0)
                return Result.Failure(errors);

            Quantity = quantity;
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result IncreaseQuantity(int amount = 1)
        {
            if (amount <= 0)
                return Result.Failure(new Error("Amount.NonPositive", "Amount must be positive."));

            Quantity += amount;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        internal void AddOption(CartItemOption option) => CartItemOptions.Add(option);
    }
}