using Domain.Common;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }

        public Guid SectionId { get; private set; }

        public Guid StoreId { get; private set; }

        public string NameEn { get; private set; } = null!;

        public string NameAr { get; private set; } = null!;

        public string? DescriptionEn { get; private set; }

        public string? DescriptionAr { get; private set; }

        public string? Metadata { get; private set; }   // jsonb as string

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

        public bool IsDeleted => DeletedAt.HasValue;
public StoreSection Section { get; private set; } = null!;
public Store Store { get; private set; } = null!;
public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
public ICollection<ProductOptionGroup> OptionGroups { get; private set; } = new List<ProductOptionGroup>();
public ICollection<DiscountProduct> DiscountProducts { get; private set; } = new List<DiscountProduct>();
public ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();
public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
public ICollection<FavoriteProduct> FavoriteProducts { get; private set; } = new List<FavoriteProduct>();

        private Product()
        {
        }

        public static Result<Product> Create(
            Guid sectionId,
            Guid storeId,
            string nameEn,
            string nameAr,
            decimal price,
            string? descriptionEn = null,
            string? descriptionAr = null,
            string? metadata = null,
            decimal? comparePrice = null,
            string? sku = null,
            string? barcode = null,
            bool trackInventory = false,
            int stockQuantity = 0,
            decimal? weight = null,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(sectionId, errors, "SectionId");
            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");

            descriptionEn = DomainValidation.NormalizeOptional(descriptionEn);
            descriptionAr = DomainValidation.NormalizeOptional(descriptionAr);
            metadata = DomainValidation.NormalizeOptional(metadata);
            sku = DomainValidation.NormalizeOptional(sku);
            barcode = DomainValidation.NormalizeOptional(barcode);

            DomainValidation.EnsureNonNegative(price, errors, "Price");
            DomainValidation.EnsureComparePriceValid(price, comparePrice, errors);
            DomainValidation.EnsureNonNegative(stockQuantity, errors, "Stock quantity");

            if (weight.HasValue)
                DomainValidation.EnsureNonNegative(weight.Value, errors, "Weight");

            if (errors.Count > 0)
                return Result<Product>.Failure(errors);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                SectionId = sectionId,
                StoreId = storeId,
                NameEn = nameEn,
                NameAr = nameAr,
                DescriptionEn = descriptionEn,
                DescriptionAr = descriptionAr,
                Metadata = metadata,
                Price = price,
                ComparePrice = comparePrice,
                Sku = sku,
                Barcode = barcode,
                TrackInventory = trackInventory,
                StockQuantity = stockQuantity,
                Weight = weight,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Product>.Success(product);
        }

        public Result Update(
            string nameEn,
            string nameAr,
            decimal price,
            string? descriptionEn = null,
            string? descriptionAr = null,
            string? metadata = null,
            decimal? comparePrice = null,
            string? sku = null,
            string? barcode = null,
            bool trackInventory = false,
            int stockQuantity = 0,
            decimal? weight = null)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");

            descriptionEn = DomainValidation.NormalizeOptional(descriptionEn);
            descriptionAr = DomainValidation.NormalizeOptional(descriptionAr);
            metadata = DomainValidation.NormalizeOptional(metadata);
            sku = DomainValidation.NormalizeOptional(sku);
            barcode = DomainValidation.NormalizeOptional(barcode);

            DomainValidation.EnsureNonNegative(price, errors, "Price");
            DomainValidation.EnsureComparePriceValid(price, comparePrice, errors);
            DomainValidation.EnsureNonNegative(stockQuantity, errors, "Stock quantity");

            if (weight.HasValue)
                DomainValidation.EnsureNonNegative(weight.Value, errors, "Weight");

            if (errors.Count > 0)
                return Result.Failure(errors);

            NameEn = nameEn;
            NameAr = nameAr;
            DescriptionEn = descriptionEn;
            DescriptionAr = descriptionAr;
            Metadata = metadata;
            Price = price;
            ComparePrice = comparePrice;
            Sku = sku;
            Barcode = barcode;
            TrackInventory = trackInventory;
            StockQuantity = stockQuantity;
            Weight = weight;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}