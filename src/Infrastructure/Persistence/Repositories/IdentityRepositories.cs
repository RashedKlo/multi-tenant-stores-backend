using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    public CustomerRepository(AppDbContext context) => _context = context;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    // Hit on login — matches idx_customers_email. Tracked, since the caller
    // (login flow) may update LastLoginAt/etc. immediately after.
    public Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Customers.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public Task<Customer?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default) =>
        _context.Customers.FirstOrDefaultAsync(c => c.GoogleId == googleId, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Customers.AsNoTracking().AnyAsync(c => c.Email == email, cancellationToken);

    public void Add(Customer customer) => _context.Customers.Add(customer);
    public void Update(Customer customer) => _context.Customers.Update(customer);
    public void Delete(Customer customer) => _context.Customers.Remove(customer);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    public RefreshTokenRepository(AppDbContext context) => _context = context;

    // Always looked up by the pre-hashed value — never pass a raw token in.
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public Task<List<RefreshToken>> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens
            .Where(t => t.CustomerId == customerId && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

    public void Add(RefreshToken token) => _context.RefreshTokens.Add(token);
    public void Update(RefreshToken token) => _context.RefreshTokens.Update(token);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly AppDbContext _context;
    public CustomerAddressRepository(AppDbContext context) => _context = context;

    public Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.CustomerAddresses.FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);

    // Ownership-scoped — the (customer_id, id) predicate mirrors the
    // composite FK on orders and means a bad/foreign id simply returns
    // null instead of ever reaching an authorization check downstream.
    public Task<CustomerAddress?> GetByIdForCustomerAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default) =>
        _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId && a.DeletedAt == null, cancellationToken);

    public Task<List<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        _context.CustomerAddresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId && a.DeletedAt == null)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<CustomerAddress?> GetDefaultByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        _context.CustomerAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault && a.DeletedAt == null, cancellationToken);

    public void Add(CustomerAddress address) => _context.CustomerAddresses.Add(address);
    public void Update(CustomerAddress address) => _context.CustomerAddresses.Update(address);
    public void Delete(CustomerAddress address) => _context.CustomerAddresses.Remove(address); // service sets DeletedAt, then calls Update — see interface note

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

public class GuestSessionRepository : IGuestSessionRepository
{
    private readonly AppDbContext _context;
    public GuestSessionRepository(AppDbContext context) => _context = context;

    public Task<GuestSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.GuestSessions.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

    public void Add(GuestSession session) => _context.GuestSessions.Add(session);
    public void Update(GuestSession session) => _context.GuestSessions.Update(session);
    public void Delete(GuestSession session) => _context.GuestSessions.Remove(session);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
