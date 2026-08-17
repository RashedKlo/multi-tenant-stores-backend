using Domain.Common;

namespace Domain.Entities
{
    public class HomeBanner
    {
        public Guid Id { get; private set; }

        public string ImageUrl { get; private set; } = null!;

        public string? TitleEn { get; private set; }

        public string? TitleAr { get; private set; }

        public string? SubtitleEn { get; private set; }

        public string? SubtitleAr { get; private set; }

        public string? ActionUrl { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsActive { get; private set; }

        private HomeBanner()
        {
        }

        public static Result<HomeBanner> Create(
            string imageUrl,
            string? titleEn = null,
            string? titleAr = null,
            string? subtitleEn = null,
            string? subtitleAr = null,
            string? actionUrl = null,
            int displayOrder = 0,
            bool isActive = true)
        {
            var errors = new List<Error>();

            imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            titleEn = DomainValidation.NormalizeOptional(titleEn);
            titleAr = DomainValidation.NormalizeOptional(titleAr);
            subtitleEn = DomainValidation.NormalizeOptional(subtitleEn);
            subtitleAr = DomainValidation.NormalizeOptional(subtitleAr);
            actionUrl = DomainValidation.NormalizeOptional(actionUrl);

            if (errors.Count > 0)
                return Result<HomeBanner>.Failure(errors);

            var banner = new HomeBanner
            {
                Id = Guid.NewGuid(),
                ImageUrl = imageUrl,
                TitleEn = titleEn,
                TitleAr = titleAr,
                SubtitleEn = subtitleEn,
                SubtitleAr = subtitleAr,
                ActionUrl = actionUrl,
                DisplayOrder = displayOrder,
                IsActive = isActive
            };

            return Result<HomeBanner>.Success(banner);
        }

        public Result Update(
            string imageUrl,
            string? titleEn = null,
            string? titleAr = null,
            string? subtitleEn = null,
            string? subtitleAr = null,
            string? actionUrl = null,
            int displayOrder = 0)
        {
            var errors = new List<Error>();

            imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            titleEn = DomainValidation.NormalizeOptional(titleEn);
            titleAr = DomainValidation.NormalizeOptional(titleAr);
            subtitleEn = DomainValidation.NormalizeOptional(subtitleEn);
            subtitleAr = DomainValidation.NormalizeOptional(subtitleAr);
            actionUrl = DomainValidation.NormalizeOptional(actionUrl);

            if (errors.Count > 0)
                return Result<HomeBanner>.Failure(errors);

            ImageUrl = imageUrl;
            TitleEn = titleEn;
            TitleAr = titleAr;
            SubtitleEn = subtitleEn;
            SubtitleAr = subtitleAr;
            ActionUrl = actionUrl;
            DisplayOrder = displayOrder;

            return Result<HomeBanner>.Success(this);
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