using Domain.Common;

namespace Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }

        public string FirstName { get; private set; } = null!;

        public string LastName { get; private set; } = null!;

        public string Email { get; private set; } = null!;

        public string? PasswordHash { get; private set; }

        public string? GoogleId { get; private set; }

        public bool IsEmailVerified { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        public bool IsDeleted => DeletedAt.HasValue;
        public ICollection<CustomerAddress> Addresses { get; private set; } = new List<CustomerAddress>();
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public ICollection<Cart> Carts { get; private set; } = new List<Cart>();
        public ICollection<FavoriteStore> FavoriteStores { get; private set; } = new List<FavoriteStore>();
        public ICollection<FavoriteProduct> FavoriteProducts { get; private set; } = new List<FavoriteProduct>();
        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        private Customer()
        {
        }

        public static Result<Customer> Create(
            string firstName,
            string lastName,
            string email,
            string? passwordHash = null,
            string? googleId = null,
            bool isEmailVerified = false,
            bool isActive = true)
        {
            var errors = new List<Error>();

            firstName = DomainValidation.NormalizeRequiredString(firstName, errors, "First name");
            lastName = DomainValidation.NormalizeRequiredString(lastName, errors, "Last name");
            email = DomainValidation.NormalizeRequiredEmail(email, errors);

            passwordHash = DomainValidation.NormalizeOptional(passwordHash);
            googleId = DomainValidation.NormalizeOptional(googleId);

       

            if (errors.Count > 0)
                return Result<Customer>.Failure(errors);

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = passwordHash,
                GoogleId = googleId,
                IsEmailVerified = isEmailVerified,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Customer>.Success(customer);
        }

        public Result Update(
            string firstName,
            string lastName,
            string email,
            string? passwordHash = null,
            string? googleId = null)
        {
            var errors = new List<Error>();

            firstName = DomainValidation.NormalizeRequiredString(firstName, errors, "First name");
            lastName = DomainValidation.NormalizeRequiredString(lastName, errors, "Last name");
            email = DomainValidation.NormalizeRequiredEmail(email, errors);

            passwordHash = DomainValidation.NormalizeOptional(passwordHash);
            googleId = DomainValidation.NormalizeOptional(googleId);


            if (errors.Count > 0)
                return Result.Failure(errors);

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            GoogleId = googleId;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            UpdatedAt = DateTime.UtcNow;
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