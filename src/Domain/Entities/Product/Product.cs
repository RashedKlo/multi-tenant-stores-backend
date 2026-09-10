using Domain.Common;

namespace Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }
    public Guid SectionId { get; private set; }
    public Guid StoreId { get; private set; }
    public string NameEn { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public string? DescriptionEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? Metadata { get; private set; }
    public decimal Price { get; private set; }
    public decimal? ComparePrice { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public bool TrackInventory { get; private set; }
    public int StockQuantity { get; private set; }
    public decimal? Weight { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
    public bool IsDeleted => DeletedAt.HasValue;

    private Product()
    {
    }

}