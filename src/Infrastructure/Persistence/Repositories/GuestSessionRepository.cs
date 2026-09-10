using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GuestSessionRepository : IGuestSessionRepository
{
    private readonly AppDbContext _context;
    public GuestSessionRepository(AppDbContext context) => _context = context;

    public Task<GuestSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _context.GuestSessions.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

    public  async Task AddAsync(GuestSession session, CancellationToken cancellationToken = default) => _context.GuestSessions.Add(session);
    public void Update(GuestSession session, CancellationToken cancellationToken = default) => _context.GuestSessions.Update(session);
    public void Delete(GuestSession session, CancellationToken cancellationToken = default) => _context.GuestSessions.Remove(session);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
