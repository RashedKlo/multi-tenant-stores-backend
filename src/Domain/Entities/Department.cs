public class Department
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Store Store { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Department() { }

    public static Department Create(Guid storeId, string name, string? imageUrl)
    {
        return new Department
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = name,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? imageUrl)
    {
        Name = name;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}