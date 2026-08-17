using Domain.Common;

namespace Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }

        public Guid OrderId { get; private set; }

        public Guid? ProductId { get; private set; }

        public string NameEnSnapshot { get; private set; } = null!;

        public string NameArSnapshot { get; private set; } = null!;

        public decimal UnitPriceSnapshot { get; private set; }

        public int Quantity { get; private set; }

        public decimal LineTotal { get; private set; }
        public Order Order { get; private set; } = null!;
        public ICollection<OrderItemOption> OrderItemOptions { get; private set; } = new List<OrderItemOption>();
        public Product? Product { get; private set; }

        private OrderItem()
        {
        }

        public static Result<OrderItem> Create(
            Guid orderId,
            string nameEnSnapshot,
            string nameArSnapshot,
            decimal unitPriceSnapshot,
            int quantity,
            Guid? productId = null)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(orderId, errors, "OrderId");

            nameEnSnapshot = DomainValidation.NormalizeRequiredString(nameEnSnapshot, errors, "NameEn snapshot");
            nameArSnapshot = DomainValidation.NormalizeRequiredString(nameArSnapshot, errors, "NameAr snapshot");

            DomainValidation.EnsureNonNegative(unitPriceSnapshot, errors, "Unit price");
            DomainValidation.EnsurePositive(quantity, errors, "Quantity");

            var lineTotal = unitPriceSnapshot * quantity;
            DomainValidation.EnsureNonNegative(lineTotal, errors, "Line total");

            if (errors.Count > 0)
                return Result<OrderItem>.Failure(errors);

            var item = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = productId,
                NameEnSnapshot = nameEnSnapshot,
                NameArSnapshot = nameArSnapshot,
                UnitPriceSnapshot = unitPriceSnapshot,
                Quantity = quantity,
                LineTotal = lineTotal
            };

            return Result<OrderItem>.Success(item);
        }
    }
}