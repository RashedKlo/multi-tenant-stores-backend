using Domain.Common;

namespace Domain.Entities
{
    public class CustomerAddress
    {
        public Guid Id { get; private set; }

        public Guid CustomerId { get; private set; }

        public string Label { get; private set; } = null!;

        public decimal Latitude { get; private set; }

        public decimal Longitude { get; private set; }

        public string AddressText { get; private set; } = null!;

        public bool IsDefault { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        public bool IsDeleted => DeletedAt.HasValue;
        public Customer Customer { get; private set; } = null!;
        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        private CustomerAddress()
        {
        }

        public static Result<CustomerAddress> Create(
            Guid customerId,
            string label,
            decimal latitude,
            decimal longitude,
            string addressText,
            bool isDefault = false)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");

            label = DomainValidation.NormalizeRequiredString(label, errors, "Label");
            addressText = DomainValidation.NormalizeRequiredString(addressText, errors, "Address text");

            DomainValidation.EnsureValidLatitude(latitude, errors);
            DomainValidation.EnsureValidLongitude(longitude, errors);

            if (errors.Count > 0)
                return Result<CustomerAddress>.Failure(errors);

            var address = new CustomerAddress
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Label = label,
                Latitude = latitude,
                Longitude = longitude,
                AddressText = addressText,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<CustomerAddress>.Success(address);
        }

        public Result<CustomerAddress> Update(
            string label,
            decimal latitude,
            decimal longitude,
            string addressText)
        {
            var errors = new List<Error>();

            label = DomainValidation.NormalizeRequiredString(label, errors, "Label");
            addressText = DomainValidation.NormalizeRequiredString(addressText, errors, "Address text");

            DomainValidation.EnsureValidLatitude(latitude, errors);
            DomainValidation.EnsureValidLongitude(longitude, errors);

            if (errors.Count > 0)
                return Result<CustomerAddress>.Failure(errors);

            Label = label;
            Latitude = latitude;
            Longitude = longitude;
            AddressText = addressText;
            UpdatedAt = DateTime.UtcNow;

            return Result<CustomerAddress>.Success(this);
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UnsetDefault()
        {
            IsDefault = false;
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