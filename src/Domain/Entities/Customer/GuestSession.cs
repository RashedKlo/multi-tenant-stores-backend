using Domain.Common;

namespace Domain.Entities
{
    public class GuestSession
    {
        public Guid Id { get; private set; }

        public string TokenHash { get; private set; } = null!;

        public DateTime CreatedAt { get; private set; }

        public DateTime LastSeenAt { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public ICollection<Cart> Carts { get; private set; } = new List<Cart>();

        private GuestSession()
        {
        }

        public static Result<GuestSession> Create(string tokenHash, DateTime expiresAt)
        {
            var errors = new List<Error>();

            tokenHash = DomainValidation.NormalizeRequiredHash(tokenHash, errors, "Token hash");

           DomainValidation.EnsureInFuture(expiresAt, errors, "GuestSessionExpiresAt");

            if (errors.Count > 0)
                return Result<GuestSession>.Failure(errors);

            var now = DateTime.UtcNow;

            var session = new GuestSession
            {
                Id = Guid.NewGuid(),
                TokenHash = tokenHash,
                CreatedAt = now,
                LastSeenAt = now,
                ExpiresAt = expiresAt
            };

            return Result<GuestSession>.Success(session);
        }

        public void Touch()
        {
            LastSeenAt = DateTime.UtcNow;
        }
    }
}