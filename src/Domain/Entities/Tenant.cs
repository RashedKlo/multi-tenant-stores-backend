using Domain.Common;

namespace Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public ICollection<Store> Stores { get; private set; } = new List<Store>();

        public bool IsDeleted => DeletedAt.HasValue;

        private Tenant()
        {
        }

        public static Result<Tenant> Create(string name, string email, string passwordHash)
        {
            var errors = new List<Error>();

            name = DomainValidation.NormalizeRequiredString(name, errors, "Name");
            email = DomainValidation.NormalizeRequiredEmail(email, errors);
            passwordHash = DomainValidation.NormalizeRequiredHash(passwordHash, errors);

            if (errors.Count > 0)
                return Result<Tenant>.Failure(errors);

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Tenant>.Success(tenant);
        }

        public Result Update(string name, string email, string passwordHash)
        {
            var errors = new List<Error>();

            name = DomainValidation.NormalizeRequiredString(name, errors, "Name");
            email = DomainValidation.NormalizeRequiredEmail(email, errors);
            passwordHash = DomainValidation.NormalizeRequiredHash(passwordHash, errors);

            if (errors.Count > 0)
                return Result.Failure(errors);

            Name = name;
            Email = email;
            PasswordHash = passwordHash;
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