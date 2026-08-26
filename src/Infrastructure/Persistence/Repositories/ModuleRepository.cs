using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly AppDbContext _context;
    public ModuleRepository(AppDbContext context) => _context = context;

    public Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Modules.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    // Public storefront read — no tracking needed, ordered exactly how the
    // home page renders the module list. Backed by is_active default index.
    public Task<List<Module>> GetActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        _context.Modules
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
            public Task<Module?> GetReadByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
    _context.Modules
        .AsNoTracking()
        .Include(m => m.ModuleBanners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder))
        .Include(m => m.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder))
        .FirstOrDefaultAsync(m => m.Id == id, ct);

    public void Add(Module module) => _context.Modules.Add(module);
    public void Update(Module module) => _context.Modules.Update(module);
    public void Delete(Module module) => _context.Modules.Remove(module);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
