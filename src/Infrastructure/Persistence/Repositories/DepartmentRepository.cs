using Domain.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Department>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .Where(d => d.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
    }

    public void Update(Department department)
    {
        _context.Departments.Update(department);
    }

    public void Delete(Department department)
    {
        _context.Departments.Remove(department);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
