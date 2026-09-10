using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Discount
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string TitleEn { get; private set; } = null!;
    public string TitleAr { get; private set; } = null!;
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Discount()
    {
    }

}