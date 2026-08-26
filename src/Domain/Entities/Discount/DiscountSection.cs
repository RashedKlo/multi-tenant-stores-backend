using Domain.Common;

namespace Domain.Entities
{
    public class DiscountSection
    {
        public Guid DiscountId { get; private set; }

        public Guid SectionId { get; private set; }
        public Discount Discount { get; private set; } = null!;
public StoreSection Section { get; private set; } = null!;
        private DiscountSection()
        {
        }

        public static Result<DiscountSection> Create(Guid discountId, Guid sectionId)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(discountId, errors, "DiscountId");
            DomainValidation.EnsureNotEmptyGuid(sectionId, errors, "SectionId");

            if (errors.Count > 0)
                return Result<DiscountSection>.Failure(errors);

            var entity = new DiscountSection
            {
                DiscountId = discountId,
                SectionId = sectionId
            };

            return Result<DiscountSection>.Success(entity);
        }
    }
}