using Domain.Common;

namespace Domain.Entities
{
    public class OrderItemOption
    {
        public Guid Id { get; private set; }

        public Guid OrderItemId { get; private set; }

        public string OptionNameEnSnapshot { get; private set; } = null!;

        public string OptionNameArSnapshot { get; private set; } = null!;

        public decimal PriceAdjustmentSnapshot { get; private set; }
        public OrderItem OrderItem { get; private set; } = null!;

        private OrderItemOption()
        {
        }

        public static Result<OrderItemOption> Create(
            Guid orderItemId,
            string optionNameEnSnapshot,
            string optionNameArSnapshot,
            decimal priceAdjustmentSnapshot = 0)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(orderItemId, errors, "OrderItemId");

            optionNameEnSnapshot = DomainValidation.NormalizeRequiredString(optionNameEnSnapshot, errors, "Option name EN");
            optionNameArSnapshot = DomainValidation.NormalizeRequiredString(optionNameArSnapshot, errors, "Option name AR");

            if (errors.Count > 0)
                return Result<OrderItemOption>.Failure(errors);

            var option = new OrderItemOption
            {
                Id = Guid.NewGuid(),
                OrderItemId = orderItemId,
                OptionNameEnSnapshot = optionNameEnSnapshot,
                OptionNameArSnapshot = optionNameArSnapshot,
                PriceAdjustmentSnapshot = priceAdjustmentSnapshot
            };

            return Result<OrderItemOption>.Success(option);
        }
    }
}