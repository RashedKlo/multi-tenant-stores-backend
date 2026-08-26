using Domain.Common;

namespace Domain.Entities
{
    public class StoreSection
    {
        public Guid Id { get; private set; }

        public Guid StoreId { get; private set; }

        public string NameEn { get; private set; } = null!;

        public string NameAr { get; private set; } = null!;

        public string? ImageUrl { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsActive { get; private set; }
        public Store Store { get; private set; } = null!;
        public ICollection<Product> Products { get; private set; } = new List<Product>();
        public ICollection<DiscountProduct> DiscountProducts { get; private set; } = new List<DiscountProduct>();
        public ICollection<DiscountSection> DiscountSections { get; private set; } = new List<DiscountSection>();

        private StoreSection()
        {
        }

        public static Result<StoreSection> Create(
            Guid storeId,
            string nameEn,
            string nameAr,
            string? imageUrl = null,
            int displayOrder = 0,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");
            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");
            imageUrl = DomainValidation.NormalizeOptional(imageUrl);

            if (errors.Count > 0)
                return Result<StoreSection>.Failure(errors);

            var section = new StoreSection
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                NameEn = nameEn,
                NameAr = nameAr,
                ImageUrl = imageUrl,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };

            return Result<StoreSection>.Success(section);
        }

        public Result Update(
            string nameEn,
            string nameAr,
            string? imageUrl = null,
            int displayOrder = 0)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            imageUrl = DomainValidation.NormalizeOptional(imageUrl);

            if (errors.Count > 0)
                return Result.Failure(errors);

            NameEn = nameEn;
            NameAr = nameAr;
            ImageUrl = imageUrl;
            DisplayOrder = displayOrder;

            return Result.Success();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}