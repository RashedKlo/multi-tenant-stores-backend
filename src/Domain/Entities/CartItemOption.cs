using Domain.Common;

namespace Domain.Entities
{
    public class CartItemOption
    {
        public Guid CartItemId { get; private set; }

        public Guid OptionId { get; private set; }
        public CartItem CartItem { get; private set; } = null!;
        public ProductOption Option { get; private set; } = null!;

        private CartItemOption()
        {
        }

        public static Result<CartItemOption> Create(Guid cartItemId, Guid optionId)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(cartItemId, errors, "CartItemId");
            DomainValidation.EnsureNotEmptyGuid(optionId, errors, "OptionId");

            if (errors.Count > 0)
                return Result<CartItemOption>.Failure(errors);

            var entity = new CartItemOption
            {
                CartItemId = cartItemId,
                OptionId = optionId
            };

            return Result<CartItemOption>.Success(entity);
        }
    }
}