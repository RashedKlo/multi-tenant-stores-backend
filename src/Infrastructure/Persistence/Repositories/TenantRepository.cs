using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;
    public TenantRepository(AppDbContext context) => _context = context;

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Tenants.AsNoTracking().AnyAsync(t => t.Email == email, cancellationToken);

    public void Add(Tenant tenant) => _context.Tenants.Add(tenant);
    public void Update(Tenant tenant) => _context.Tenants.Update(tenant);
    public void Delete(Tenant tenant) => _context.Tenants.Remove(tenant);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
