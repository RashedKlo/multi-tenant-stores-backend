using Domain.Entities;

namespace Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Customer?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(Customer customer);
    void Update(Customer customer);
    void Delete(Customer customer);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository
{
    // tokenHash — never look these up by raw token; hash first in the service.
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    void Add(RefreshToken token);
    void Update(RefreshToken token); // e.g. setting RevokedAt / LastUsedAt

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICustomerAddressRepository
{
    Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Enforces the ownership check at the query level too (belt-and-suspenders
    // alongside the composite FK on orders) — pass the caller's own customerId.
    Task<CustomerAddress?> GetByIdForCustomerAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default);
// ICustomerAddressRepository
Task UnsetDefaultForCustomerAsync(Guid customerId, Guid exceptAddressId, CancellationToken ct = default);
    Task<List<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerAddress?> GetDefaultByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    void Add(CustomerAddress address);
    void Update(CustomerAddress address);
    void Delete(CustomerAddress address); // soft delete — set DeletedAt in the service, then call Update instead

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IGuestSessionRepository
{
    Task<GuestSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    void Add(GuestSession session);
    void Update(GuestSession session); // e.g. bumping LastSeenAt
    void Delete(GuestSession session); // expired cleanup job

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
