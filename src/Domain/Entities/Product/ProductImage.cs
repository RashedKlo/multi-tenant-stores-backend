using Domain.Common;

namespace Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public string ImageUrl { get; private set; } = null!;

        public int DisplayOrder { get; private set; }

        private ProductImage()
        {
        }

   
}
}