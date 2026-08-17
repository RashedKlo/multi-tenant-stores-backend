using Domain.Common;

namespace Domain.Entities
{
    public class ProductOption
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
        public ProductOptionGroup OptionGroup { get; private set; } = null!;
        public ICollection<CartItemOption> CartItemOptions { get; private set; } = new List<CartItemOption>();

        private ProductOption()
        {
        }

        public static Result<ProductOption> Create(
            Guid optionGroupId,
            string nameEn,
            string nameAr,
            decimal priceAdjustment = 0,
            bool isDefault = false,
            int displayOrder = 0,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(optionGroupId, errors, "OptionGroupId");

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");
            DomainValidation.EnsureNonNegative(priceAdjustment, errors, "Price adjustment");

            if (errors.Count > 0)
                return Result<ProductOption>.Failure(errors);

            var option = new ProductOption
            {
                Id = Guid.NewGuid(),
                OptionGroupId = optionGroupId,
                NameEn = nameEn,
                NameAr = nameAr,
                PriceAdjustment = priceAdjustment,
                IsDefault = isDefault,
                DisplayOrder = displayOrder,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<ProductOption>.Success(option);
        }

        public Result Update(
            string nameEn,
            string nameAr,
            decimal priceAdjustment,
            bool isDefault,
            int displayOrder)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            if (errors.Count > 0)
                return Result.Failure(errors);

            NameEn = nameEn;
            NameAr = nameAr;
            PriceAdjustment = priceAdjustment;
            IsDefault = isDefault;
            DisplayOrder = displayOrder;
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