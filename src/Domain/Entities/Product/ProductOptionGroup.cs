using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class ProductOptionGroup
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public SelectionType SelectionType { get; private set; }
    public int MinSelection { get; private set; }
    public int MaxSelection { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    private ProductOptionGroup()
    {
    }

}