using Domain.Common;

namespace Domain.Entities
{
    public class Category
    {
        public Guid Id { get; private set; }

        public Guid ModuleId { get; private set; }

        public string NameEn { get; private set; } = null!;

        public string NameAr { get; private set; } = null!;

        public string? ImageUrl { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsActive { get; private set; }
public Module Module { get; private set; } = null!;
public ICollection<StoreCategory> StoreCategories { get; private set; } = new List<StoreCategory>();
        private Category()
        {
        }

        public static Result<Category> Create(
            Guid moduleId,
            string nameEn,
            string nameAr,
            string? imageUrl = null,
            int displayOrder = 0,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(moduleId, errors, "ModuleId");

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            imageUrl = DomainValidation.NormalizeOptional(imageUrl);

            if (errors.Count > 0)
                return Result<Category>.Failure(errors);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                NameEn = nameEn,
                NameAr = nameAr,
                ImageUrl = imageUrl,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };

            return Result<Category>.Success(category);
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