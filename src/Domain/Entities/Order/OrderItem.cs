// Domain/Entities/Order/OrderItem.cs
using Domain.Common;

namespace Domain.Entities;

public sealed class OrderItem
{
    private readonly List<OrderItemOption> _options = [];

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid? ProductId { get; private set; }
    public string NameEnSnapshot { get; private set; } = null!;
    public string NameArSnapshot { get; private set; } = null!;
    public decimal UnitPriceSnapshot { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal { get; private set; }

    public IReadOnlyCollection<OrderItemOption> Options => _options.AsReadOnly();
    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    public static Result<OrderItem> Create(
        Guid orderId,
        string nameEnSnapshot,
        string nameArSnapshot,
        decimal unitPriceSnapshot,
        int quantity,
        Guid? productId = null,
        IReadOnlyList<OrderOptionInput>? options = null)
    {
        if (orderId == Guid.Empty)
            return Result<OrderItem>.Failure(Error.Validation("OrderItem.OrderId.Required", "OrderId is required."));
        if (string.IsNullOrWhiteSpace(nameEnSnapshot) || string.IsNullOrWhiteSpace(nameArSnapshot))
            return Result<OrderItem>.Failure(Error.Validation("OrderItem.Name.Required", "Product name snapshot is required."));
        if (unitPriceSnapshot < 0)
            return Result<OrderItem>.Failure(Error.Validation("OrderItem.UnitPrice.Invalid", "Unit price cannot be negative."));
        if (quantity <= 0)
            return Result<OrderItem>.Failure(Error.Validation("OrderItem.Quantity.Invalid", "Quantity must be positive."));

        var optionsTotal = options?.Sum(o => o.PriceAdjustment) ?? 0m;
        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            NameEnSnapshot = nameEnSnapshot.Trim(),
            NameArSnapshot = nameArSnapshot.Trim(),
            UnitPriceSnapshot = unitPriceSnapshot,
            Quantity = quantity,
            LineTotal = (unitPriceSnapshot + optionsTotal) * quantity
        };

        if (options is not null)
        {
            foreach (var opt in options)
            {
                var optResult = OrderItemOption.Create(
                    item.Id, opt.NameEn, opt.NameAr, opt.PriceAdjustment);
                if (optResult.IsFailure)
                    return Result<OrderItem>.Failure(optResult.Errors);
                item._options.Add(optResult.Value!);
            }
        }

        return Result<OrderItem>.Success(item);
    }
}