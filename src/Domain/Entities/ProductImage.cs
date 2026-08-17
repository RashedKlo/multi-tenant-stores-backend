using Domain.Common;

namespace Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public string ImageUrl { get; private set; } = null!;

        public int DisplayOrder { get; private set; }
        public Product Product { get; private set; } = null!;

        private ProductImage()
        {
        }

        public static Result<ProductImage> Create(
            Guid productId,
            string imageUrl,
            int displayOrder = 0)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(productId, errors, "ProductId");

            imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            if (errors.Count > 0)
                return Result<ProductImage>.Failure(errors);

            var image = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImageUrl = imageUrl,
                DisplayOrder = displayOrder
            };

            return Result<ProductImage>.Success(image);
        }

        public Result Update(string imageUrl, int displayOrder = 0)
        {
            var errors = new List<Error>();

            imageUrl = DomainValidation.NormalizeRequiredString(imageUrl, errors, "Image URL");
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            if (errors.Count > 0)
                return Result.Failure(errors);

            ImageUrl = imageUrl;
            DisplayOrder = displayOrder;

            return Result.Success();
        }
    }
}