using Domain.Common;

namespace Domain.Entities
{
    public class DiscountProduct
    {
        public Guid DiscountId { get; private set; }

        public Guid ProductId { get; private set; }
        public Discount Discount { get; private set;}= null!;
        public Product Product { get; private set; }= null!;

        private DiscountProduct()
        {
        }

        public static Result<DiscountProduct> Create(Guid discountId, Guid productId)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(discountId, errors, "DiscountId");
            DomainValidation.EnsureNotEmptyGuid(productId, errors, "ProductId");

            if (errors.Count > 0)
                return Result<DiscountProduct>.Failure(errors);

            var entity = new DiscountProduct
            {
                DiscountId = discountId,
                ProductId = productId
            };

            return Result<DiscountProduct>.Success(entity);
        }
    }
}