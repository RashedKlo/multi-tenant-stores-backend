using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly AppDbContext _context;

    public CustomerAddressRepository(AppDbContext context) => _context = context;

    public Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, ct);

    public Task<CustomerAddress?> GetByIdForCustomerAsync(
        Guid id,
        Guid customerId,
        CancellationToken ct = default) =>
        _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == customerId && a.DeletedAt == null, ct);

    public Task<CustomerAddress?> GetDefaultForCustomerAsync(
        Guid customerId,
        CancellationToken ct = default) =>
        _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault && a.DeletedAt == null, ct);

    public async Task<IReadOnlyList<CustomerAddress>> GetActiveByCustomerAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        var addresses = await _context.CustomerAddresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId && a.DeletedAt == null)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(ct);

        return addresses;
    }

    public async Task AddAsync(CustomerAddress address, CancellationToken ct = default) =>
        await _context.CustomerAddresses.AddAsync(address, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
