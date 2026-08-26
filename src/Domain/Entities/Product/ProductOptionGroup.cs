using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class ProductOptionGroup
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public string NameEn { get; private set; } = null!;

        public string NameAr { get; private set; } = null!;

        public SelectionType SelectionType { get; private set; }

        public int MinSelection { get; private set; }

        public int MaxSelection { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        public bool IsDeleted => DeletedAt.HasValue;
        public Product Product { get; private set; } = null!;
        public ICollection<ProductOption> Options { get; private set; } = new List<ProductOption
>();

        private ProductOptionGroup()
        {
        }

        public static Result<ProductOptionGroup> Create(
            Guid productId,
            string nameEn,
            string nameAr,
            SelectionType selectionType = SelectionType.Single,
            int minSelection = 0,
            int maxSelection = 1,
            int displayOrder = 0,
            bool isActive = true)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(productId, errors, "ProductId");

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");

            DomainValidation.EnsureSelectionBounds(minSelection, maxSelection, errors);
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

            if (errors.Count > 0)
                return Result<ProductOptionGroup>.Failure(errors);

            var group = new ProductOptionGroup
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                NameEn = nameEn,
                NameAr = nameAr,
                SelectionType = selectionType,
                MinSelection = minSelection,
                MaxSelection = maxSelection,
                DisplayOrder = displayOrder,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<ProductOptionGroup>.Success(group);
        }

        public Result Update(
            string nameEn,
            string nameAr,
            SelectionType selectionType,
            int minSelection,
            int maxSelection,
            int displayOrder)
        {
            var errors = new List<Error>();

            nameEn = DomainValidation.NormalizeRequiredString(nameEn, errors, "NameEn");
            nameAr = DomainValidation.NormalizeRequiredString(nameAr, errors, "NameAr");

            DomainValidation.EnsureSelectionBounds(minSelection, maxSelection, errors);
            DomainValidation.EnsureNonNegative(displayOrder, errors, "Display order");

          
            if (errors.Count > 0)
                return Result.Failure(errors);

            NameEn = nameEn;
            NameAr = nameAr;
            SelectionType = selectionType;
            MinSelection = minSelection;
            MaxSelection = maxSelection;
            DisplayOrder = displayOrder;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}