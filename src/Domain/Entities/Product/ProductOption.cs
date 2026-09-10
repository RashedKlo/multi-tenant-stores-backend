using Domain.Common;

namespace Domain.Entities;

public sealed class ProductOption
{
    public Guid Id { get; private set; }
    public Guid OptionGroupId { get; private set; }
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public decimal PriceAdjustment { get; private set; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    private ProductOption()
    {
    }


}