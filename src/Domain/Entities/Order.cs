using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }

        public Guid CustomerId { get; private set; }

        public Guid StoreId { get; private set; }

        public Guid? AddressId { get; private set; }

        public string DeliveryName { get; private set; } = null!;

        public string? DeliveryPhone { get; private set; }

        public string DeliveryAddressText { get; private set; } = null!;

        public decimal DeliveryLatitude { get; private set; }

        public decimal DeliveryLongitude { get; private set; }

        public OrderStatus Status { get; private set; }

        public decimal Subtotal { get; private set; }

        public decimal DiscountTotal { get; private set; }

        public decimal Total { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }
        public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> OrderStatusHistories { get; private set; } = new List<OrderStatusHistory>();
        public Payment? Payment { get; private set; }
        public Customer Customer { get; private set; } = null!;
        public Store Store { get; private set; } = null!;
        public CustomerAddress? CustomerAddress { get; private set; }

        private Order()
        {
        }

        public static Result<Order> Create(
            Guid customerId,
            Guid storeId,
            string deliveryName,
            string deliveryAddressText,
            decimal deliveryLatitude,
            decimal deliveryLongitude,
            decimal subtotal,
            decimal discountTotal = 0,
            Guid? addressId = null,
            string? deliveryPhone = null)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");


            deliveryName = DomainValidation.NormalizeRequiredString(deliveryName, errors, "Delivery name");
            deliveryAddressText = DomainValidation.NormalizeRequiredString(deliveryAddressText, errors, "Delivery address");
            deliveryPhone = DomainValidation.NormalizeOptional(deliveryPhone);

            DomainValidation.EnsureValidLatitude(deliveryLatitude, errors, "Delivery latitude");
            DomainValidation.EnsureValidLongitude(deliveryLongitude, errors, "Delivery longitude");

            DomainValidation.EnsureNonNegative(subtotal, errors, "Subtotal");
            DomainValidation.EnsureNonNegative(discountTotal, errors, "Discount total");
            var total = subtotal - discountTotal;
            DomainValidation.EnsureNonNegative(total, errors, "Total");

            if (errors.Count > 0)
                return Result<Order>.Failure(errors);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                StoreId = storeId,
                AddressId = addressId,
                DeliveryName = deliveryName,
                DeliveryPhone = deliveryPhone,
                DeliveryAddressText = deliveryAddressText,
                DeliveryLatitude = deliveryLatitude,
                DeliveryLongitude = deliveryLongitude,
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                DiscountTotal = discountTotal,
                Total = total,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Order>.Success(order);
        }

        public Result ChangeStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}