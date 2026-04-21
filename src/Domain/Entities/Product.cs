public class Product
{

    public Guid Id { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal? DiscountPrice { get; private set; }
    public int Stock { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? SKU { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Department Department { get; private set; }

    private Product() { }

    public static Product Create(
        Guid departmentId,
        string title,
        string? description,
        decimal price,
        int stock,
        string? imageUrl,
        string? sku)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            DepartmentId = departmentId,
            Title = title,
            Description = description,
            Price = price,
            Stock = stock,
            ImageUrl = imageUrl,
            SKU = sku,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string title,
        string? description,
        decimal price,
        decimal? discountPrice,
        int stock,
        string? imageUrl)
    {
        Title = title;
        Description = description;
        Price = price;
        DiscountPrice = discountPrice;
        Stock = stock;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}