using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    public RefreshTokenRepository(AppDbContext context) => _context = context;

    // Always looked up by the pre-hashed value — never pass a raw token in.
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public Task<List<RefreshToken>> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens
            .Where(t => t.CustomerId == customerId && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

    public  async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default) => _context.RefreshTokens.Add(token);
    public void Update(RefreshToken token) => _context.RefreshTokens.Update(token);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
