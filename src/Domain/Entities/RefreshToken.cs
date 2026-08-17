using Domain.Common;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }

        public Guid CustomerId { get; private set; }

        public string TokenHash { get; private set; } = null!;

        public DateTime ExpiresAt { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        public DateTime? LastUsedAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsActive => !IsRevoked && !IsExpired;
        public Customer Customer { get; private set; } = null!;

        private RefreshToken()
        {
        }

        public static Result<RefreshToken> Create(
            Guid customerId,
            string tokenHash,
            DateTime expiresAt)
        {
            var errors = new List<Error>();
            DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");
            tokenHash = DomainValidation.NormalizeRequiredHash(tokenHash, errors, "Token hash");
          DomainValidation.EnsureInFuture(expiresAt, errors, "RefreshTokenExpiresAt");

            if (errors.Count > 0)
                return Result<RefreshToken>.Failure(errors);

            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            return Result<RefreshToken>.Success(token);
        }

        public void MarkUsed()
        {
            LastUsedAt = DateTime.UtcNow;
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}