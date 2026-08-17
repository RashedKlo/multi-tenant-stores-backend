using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Discount
    {
        public Guid Id { get; private set; }

        public Guid StoreId { get; private set; }

        public string TitleEn { get; private set; } = null!;

        public string TitleAr { get; private set; } = null!;

        public DiscountType Type { get; private set; }

        public decimal Value { get; private set; }

        public DateTime? StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public bool IsActive { get; private set; }
    public ICollection<DiscountProduct> DiscountProducts { get; private set; } = new List<DiscountProduct>();
    public ICollection<DiscountSection> DiscountSections { get; private set; } = new List<DiscountSection>();
    public Store Store { get; private set; } = null!;

        private Discount()
        {
        }

        public static Result<Discount> Create(
            Guid storeId,
            string titleEn,
            string titleAr,
            DiscountType type,
            decimal value,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");
            titleEn = DomainValidation.NormalizeRequiredString(titleEn, errors, "TitleEn");
            titleAr = DomainValidation.NormalizeRequiredString(titleAr, errors, "TitleAr");

            DomainValidation.EnsurePositive(value, errors, "Value");

            if (type == DiscountType.Percentage)
                DomainValidation.EnsureValidPercentage(value, errors, "Value");

            DomainValidation.EnsureValidDateRange(startDate, endDate, errors);

            if (errors.Count > 0)
                return Result<Discount>.Failure(errors);

            var discount = new Discount
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                TitleEn = titleEn,
                TitleAr = titleAr,
                Type = type,
                Value = value,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = isActive
            };

            return Result<Discount>.Success(discount);
        }

        public Result Update(
            string titleEn,
            string titleAr,
            DiscountType type,
            decimal value,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var errors = new List<Error>();

            titleEn = DomainValidation.NormalizeRequiredString(titleEn, errors, "TitleEn");
            titleAr = DomainValidation.NormalizeRequiredString(titleAr, errors, "TitleAr");

            DomainValidation.EnsurePositive(value, errors, "Value");

            if (type == DiscountType.Percentage)
                DomainValidation.EnsureValidPercentage(value, errors, "Value");

            DomainValidation.EnsureValidDateRange(startDate, endDate, errors);

            if (errors.Count > 0)
                return Result.Failure(errors);

            TitleEn = titleEn;
            TitleAr = titleAr;
            Type = type;
            Value = value;
            StartDate = startDate;
            EndDate = endDate;

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