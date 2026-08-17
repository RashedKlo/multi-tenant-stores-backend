using Domain.Common;

namespace Domain.Entities
{
    public class Store
    {
        public Guid Id { get; private set; }

        public Guid TenantId { get; private set; }

        public Guid ModuleId { get; private set; }

        public string NameEn { get; private set; } = null!;

        public string NameAr { get; private set; } = null!;

        public string? DescriptionEn { get; private set; }

        public string? DescriptionAr { get; private set; }

        public string? LogoUrl { get; private set; }

        public string? BannerUrl { get; private set; }

        public string? Phone { get; private set; }

        public string? Email { get; private set; }

        public string? AddressEn { get; private set; }

        public string? AddressAr { get; private set; }

        public decimal? Latitude { get; private set; }

        public decimal? Longitude { get; private set; }

        public decimal Rating { get; private set; }

        public string? Metadata { get; private set; }   // jsonb stored as string

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        public bool IsDeleted => DeletedAt.HasValue;
        public Tenant Tenant { get; private set; } = null!;
        public Module Module { get; private set; } = null!;

        public ICollection<StoreCategory> StoreCategories { get; private set; } = new List<StoreCategory>();
        public ICollection<StoreSection> StoreSections { get; private set; } = new List<StoreSection>();
        public ICollection<Product> Products { get; private set; } = new List<Product>();
        public ICollection<Discount> Discounts { get; private set; } = new List<Discount>();
        public ICollection<Cart> Carts { get; private set; } = new List<Cart>();
        public ICollection<Order> Orders { get; private set; } = new List<Order>();
        public ICollection<StoreBanner> StoreBanners { get; private set; } = new List<StoreBanner>();
 public ICollection<FavoriteStore> FavoriteStores { get; private set; } = new List<FavoriteStore>();
            private Store()
        {
        }

        public static Result<Store> Create(
            Guid tenantId,
            Guid moduleId,
            string nameEn,
            string nameAr,
            string? descriptionEn = null,
            string? descriptionAr = null,
            string? logoUrl = null,
            string? bannerUrl = null,
            string? phone = null,
            string? email = null,
            string? addressEn = null,
            string? addressAr = null,
            decimal? latitude = null,
            decimal? longitude = null,
            decimal rating = 0,
            string? metadata = null,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(tenantId, errors, "TenantId");
            DomainValidation.EnsureNotEmptyGuid(moduleId, errors, "ModuleId");

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");

            descriptionEn = DomainValidation.NormalizeOptional(descriptionEn);
            descriptionAr = DomainValidation.NormalizeOptional(descriptionAr);
            logoUrl = DomainValidation.NormalizeOptional(logoUrl);
            bannerUrl = DomainValidation.NormalizeOptional(bannerUrl);
            phone = DomainValidation.NormalizeOptional(phone);
            addressEn = DomainValidation.NormalizeOptional(addressEn);
            addressAr = DomainValidation.NormalizeOptional(addressAr);
            metadata = DomainValidation.NormalizeOptional(metadata);

            email = DomainValidation.NormalizeOptionalEmail(email, errors);

            DomainValidation.EnsureValidLatitude(latitude, errors);
            DomainValidation.EnsureValidLongitude(longitude, errors);
            DomainValidation.EnsureValidRating(rating, errors);

            if (errors.Count > 0)
                return Result<Store>.Failure(errors);

            var store = new Store
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ModuleId = moduleId,
                NameEn = nameEn,
                NameAr = nameAr,
                DescriptionEn = descriptionEn,
                DescriptionAr = descriptionAr,
                LogoUrl = logoUrl,
                BannerUrl = bannerUrl,
                Phone = phone,
                Email = email,
                AddressEn = addressEn,
                AddressAr = addressAr,
                Latitude = latitude,
                Longitude = longitude,
                Rating = rating,
                Metadata = metadata,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Store>.Success(store);
        }

        public Result Update(
            string nameEn,
            string nameAr,
            string? descriptionEn = null,
            string? descriptionAr = null,
            string? logoUrl = null,
            string? bannerUrl = null,
            string? phone = null,
            string? email = null,
            string? addressEn = null,
            string? addressAr = null,
            decimal? latitude = null,
            decimal? longitude = null,
            decimal rating = 0,
            string? metadata = null)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");

            descriptionEn = DomainValidation.NormalizeOptional(descriptionEn);
            descriptionAr = DomainValidation.NormalizeOptional(descriptionAr);
            logoUrl = DomainValidation.NormalizeOptional(logoUrl);
            bannerUrl = DomainValidation.NormalizeOptional(bannerUrl);
            phone = DomainValidation.NormalizeOptional(phone);
            addressEn = DomainValidation.NormalizeOptional(addressEn);
            addressAr = DomainValidation.NormalizeOptional(addressAr);
            metadata = DomainValidation.NormalizeOptional(metadata);

            email = DomainValidation.NormalizeOptionalEmail(email, errors);

            DomainValidation.EnsureValidLatitude(latitude, errors);
            DomainValidation.EnsureValidLongitude(longitude, errors);
            DomainValidation.EnsureValidRating(rating, errors);

            if (errors.Count > 0)
                return Result.Failure(errors);

            NameEn = nameEn;
            NameAr = nameAr;
            DescriptionEn = descriptionEn;
            DescriptionAr = descriptionAr;
            LogoUrl = logoUrl;
            BannerUrl = bannerUrl;
            Phone = phone;
            Email = email;
            AddressEn = addressEn;
            AddressAr = addressAr;
            Latitude = latitude;
            Longitude = longitude;
            Rating = rating;
            Metadata = metadata;
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