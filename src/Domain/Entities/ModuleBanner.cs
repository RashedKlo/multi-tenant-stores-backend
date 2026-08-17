using Domain.Common;

namespace Domain.Entities
{
    public class ModuleBanner
    {
        public Guid Id { get; private set; }

        public Guid ModuleId { get; private set; }

        public string ImageUrl { get; private set; } = null!;

        public string? TitleEn { get; private set; }

        public string? TitleAr { get; private set; }

        public string? ActionUrl { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsActive { get; private set; }

        private ModuleBanner()
        {
        }

        public static Result<ModuleBanner> Create(
            Guid moduleId,
            string imageUrl,
            string? titleEn = null,
            string? titleAr = null,
            string? actionUrl = null,
            int displayOrder = 0,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(moduleId, errors, "ModuleId");

            imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            titleEn = DomainValidation.NormalizeOptional(titleEn);
            titleAr = DomainValidation.NormalizeOptional(titleAr);
            actionUrl = DomainValidation.NormalizeOptional(actionUrl);

            if (errors.Count > 0)
                return Result<ModuleBanner>.Failure(errors);

            var banner = new ModuleBanner
            {
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                ImageUrl = imageUrl,
                TitleEn = titleEn,
                TitleAr = titleAr,
                ActionUrl = actionUrl,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };

            return Result<ModuleBanner>.Success(banner);
        }

        public Result Update(
            string imageUrl,
            string? titleEn = null,
            string? titleAr = null,
            string? actionUrl = null,
            int displayOrder = 0)
        {
            var errors = new List<Error>();

            imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            titleEn = DomainValidation.NormalizeOptional(titleEn);
            titleAr = DomainValidation.NormalizeOptional(titleAr);
            actionUrl = DomainValidation.NormalizeOptional(actionUrl);

            if (errors.Count > 0)
                return Result<ModuleBanner>.Failure(errors);

            ImageUrl = imageUrl;
            TitleEn = titleEn;
            TitleAr = titleAr;
            ActionUrl = actionUrl;
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