// Domain/Entities/Order/Order.cs
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

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

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public Customer Customer { get; private set; } = null!;
    public Store Store { get; private set; } = null!;
    public CustomerAddress? Address { get; private set; }
    public Payment? Payment { get; private set; }

    private Order() { }

    public static Result<Order> Create(
        Guid customerId,
        Guid storeId,
        string deliveryName,
        string deliveryAddressText,
        decimal deliveryLatitude,
        decimal deliveryLongitude,
        IReadOnlyList<OrderLineInput> lines,
        Guid? addressId = null,
        string? deliveryPhone = null,
        decimal discountTotal = 0)
    {
        if (lines is null || lines.Count == 0)
            return Result<Order>.Failure(Error.Validation(
                "Order.Items.Required", "Order must contain at least one item."));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return order
            .SetCustomerId(customerId)
            .Bind(() => order.SetStoreId(storeId))
            .Bind(() => order.SetDelivery(
                deliveryName, deliveryPhone, deliveryAddressText,
                deliveryLatitude, deliveryLongitude, addressId))
            .Bind(() => order.AddLines(lines))
            .Bind(() => order.ApplyDiscount(discountTotal))
            .Bind(() => order.RecordStatus(
                OrderStatus.Pending,
                "Order created — awaiting payment",
                ChangedByType.Customer,
                customerId))
            .Bind(() => Result<Order>.Success(order));
    }

    private Result SetCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure(Error.Validation("Order.CustomerId.Required", "CustomerId is required."));
        CustomerId = customerId;
        return Result.Success();
    }

    private Result SetStoreId(Guid storeId)
    {
        if (storeId == Guid.Empty)
            return Result.Failure(Error.Validation("Order.StoreId.Required", "StoreId is required."));
        StoreId = storeId;
        return Result.Success();
    }

    private Result SetDelivery(
        string name, string? phone, string addressText,
        decimal lat, decimal lng, Guid? addressId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Order.DeliveryName.Required", "Delivery name is required."));
        if (string.IsNullOrWhiteSpace(addressText))
            return Result.Failure(Error.Validation("Order.DeliveryAddress.Required", "Delivery address is required."));
        if (lat is < -90 or > 90)
            return Result.Failure(Error.Validation("Order.Latitude.Invalid", "Invalid latitude."));
        if (lng is < -180 or > 180)
            return Result.Failure(Error.Validation("Order.Longitude.Invalid", "Invalid longitude."));

        DeliveryName = name.Trim();
        DeliveryPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        DeliveryAddressText = addressText.Trim();
        DeliveryLatitude = lat;
        DeliveryLongitude = lng;
        AddressId = addressId;
        return Result.Success();
    }

    private Result AddLines(IReadOnlyList<OrderLineInput> lines)
    {
        decimal subtotal = 0;

        foreach (var line in lines)
        {
            var itemResult = OrderItem.Create(
                Id,
                line.NameEn,
                line.NameAr,
                line.UnitPrice,
                line.Quantity,
                line.ProductId,
                line.Options);

            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Errors);

            _items.Add(itemResult.Value!);
            subtotal += itemResult.Value!.LineTotal;
        }

        Subtotal = subtotal;
        return Result.Success();
    }

    private Result ApplyDiscount(decimal discountTotal)
    {
        if (discountTotal < 0)
            return Result.Failure(Error.Validation("Order.Discount.Invalid", "Discount cannot be negative."));
        if (discountTotal > Subtotal)
            return Result.Failure(Error.Validation("Order.Discount.TooHigh", "Discount cannot exceed subtotal."));

        DiscountTotal = discountTotal;
        Total = Subtotal - DiscountTotal;
        return Result.Success();
    }

    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Preparing, OrderStatus.Cancelled],
        [OrderStatus.Preparing] = [OrderStatus.OutForDelivery, OrderStatus.Cancelled],
        [OrderStatus.OutForDelivery] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };

    public Result ChangeStatus(
        OrderStatus newStatus,
        string? note = null,
        ChangedByType? changedByType = null,
        Guid? changedById = null)
    {
        if (Status == newStatus)
            return Result.Success();

        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            return Result.Failure(Error.Validation(
                "Order.Status.InvalidTransition",
                $"Cannot change status from {Status} to {newStatus}."));

        Status = newStatus;
        Touch();
        return RecordStatus(newStatus, note, changedByType, changedById);
    }

    public Result ConfirmPaymentReceived()
        => ChangeStatus(
            OrderStatus.Confirmed,
            "Payment succeeded",
            ChangedByType.System);

    public Result Cancel(string? reason = null, ChangedByType? by = null, Guid? byId = null)
        => ChangeStatus(OrderStatus.Cancelled, reason ?? "Cancelled", by ?? ChangedByType.System, byId);

    private Result RecordStatus(
        OrderStatus status, string? note, ChangedByType? byType, Guid? byId)
    {
        var historyResult = OrderStatusHistory.Create(Id, status, note, byType, byId);
        if (historyResult.IsFailure)
            return Result.Failure(historyResult.Errors);

        _statusHistory.Add(historyResult.Value!);
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}

public sealed record OrderLineInput(
    Guid? ProductId,
    string NameEn,
    string NameAr,
    decimal UnitPrice,
    int Quantity,
    IReadOnlyList<OrderOptionInput> Options);

public sealed record OrderOptionInput(
    string NameEn,
    string NameAr,
    decimal PriceAdjustment);