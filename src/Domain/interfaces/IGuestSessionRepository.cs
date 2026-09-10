// Domain/Interfaces/IGuestSessionRepository.cs
using Domain.Entities;

namespace Domain.Interfaces;

public interface IGuestSessionRepository
{
    Task AddAsync(GuestSession session, CancellationToken ct = default);
    Task<GuestSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}