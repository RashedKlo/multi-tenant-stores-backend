using Domain.Common;

namespace Domain.Entities
{
    public class StoreCategory
    {
        public Guid StoreId { get; private set; }

        public Guid CategoryId { get; private set; }
        public Store Store { get; private set; } = null!;
        public Category Category { get; private set; } = null!;

        private StoreCategory()
        {
        }

        public static Result<StoreCategory> Create(Guid storeId, Guid categoryId)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(storeId, errors, "StoreId");
            DomainValidation.EnsureNotEmptyGuid(categoryId, errors, "CategoryId");

            if (errors.Count > 0)
                return Result<StoreCategory>.Failure(errors);

            var storeCategory = new StoreCategory
            {
                StoreId = storeId,
                CategoryId = categoryId
            };
            return Result<StoreCategory>.Success(storeCategory);
        }
    }
}