// Domain/Entities/Order/OrderItemOption.cs
using Domain.Common;

namespace Domain.Entities;

public sealed class OrderItemOption
{
    public Guid Id { get; private set; }
    public Guid OrderItemId { get; private set; }
    public string OptionNameEnSnapshot { get; private set; } = null!;
    public string OptionNameArSnapshot { get; private set; } = null!;
    public decimal PriceAdjustmentSnapshot { get; private set; }

    public OrderItem OrderItem { get; private set; } = null!;

    private OrderItemOption() { }

    public static Result<OrderItemOption> Create(
        Guid orderItemId,
        string nameEn,
        string nameAr,
        decimal priceAdjustment = 0)
    {
        if (orderItemId == Guid.Empty)
            return Result<OrderItemOption>.Failure(Error.Validation(
                "OrderItemOption.OrderItemId.Required", "OrderItemId is required."));
        if (string.IsNullOrWhiteSpace(nameEn) || string.IsNullOrWhiteSpace(nameAr))
            return Result<OrderItemOption>.Failure(Error.Validation(
                "OrderItemOption.Name.Required", "Option name snapshot is required."));

        return Result<OrderItemOption>.Success(new OrderItemOption
        {
            Id = Guid.NewGuid(),
            OrderItemId = orderItemId,
            OptionNameEnSnapshot = nameEn.Trim(),
            OptionNameArSnapshot = nameAr.Trim(),
            PriceAdjustmentSnapshot = priceAdjustment
        });
    }
}