using Domain.Common;

namespace Domain.Entities
{
    public class Module
    {
        public Guid Id { get; private set; }
        public string NameEn { get; private set; } = string.Empty;
        public string NameAr { get; private set; } = string.Empty;
        public string? IconUrl { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }
public ICollection<Category> Categories { get; private set; } = new List<Category>();
public ICollection<ModuleBanner> ModuleBanners { get; private set; } = new List<ModuleBanner>();
public ICollection<Store> Stores { get; private set; } = new List<Store>();
        private Module()
        {
        }

        public static Result<Module> Create(
            string nameEn,
            string nameAr,
            string? iconUrl = null,
            int displayOrder = 0)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "DisplayOrder");

            iconUrl = DomainValidation.NormalizeOptional(iconUrl);

            if (errors.Count > 0)
                return Result<Module>.Failure(errors);

            var module = new Module
            {
                Id = Guid.NewGuid(),
                NameEn = nameEn,
                NameAr = nameAr,
                IconUrl = iconUrl,
                DisplayOrder = displayOrder,
                IsActive = true
            };

            return Result<Module>.Success(module);
        }

        public Result Update(
            string nameEn,
            string nameAr,
            string? iconUrl,
            int displayOrder,
            bool isActive)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "DisplayOrder");

            iconUrl = DomainValidation.NormalizeOptional(iconUrl);

            if (errors.Count > 0)
                return Result<Module>.Failure(errors);

            NameEn = nameEn;
            NameAr = nameAr;
            IconUrl = iconUrl;
            DisplayOrder = displayOrder;
            IsActive = isActive;

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