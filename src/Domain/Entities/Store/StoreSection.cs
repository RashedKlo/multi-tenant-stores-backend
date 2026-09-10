using Domain.Common;

namespace Domain.Entities;

public sealed class StoreSection
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private StoreSection()
    {
    }

}