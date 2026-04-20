using Domain.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _context;

    public TenantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(Tenant Tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(Tenant, cancellationToken);
    }

    public void Update(Tenant Tenant)
    {
        _context.Tenants.Update(Tenant);
    }

    public void Delete(Tenant Tenant)
    {
        _context.Tenants.Remove(Tenant);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}