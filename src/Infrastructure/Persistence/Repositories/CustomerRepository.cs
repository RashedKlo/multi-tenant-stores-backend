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

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Customers.AsNoTracking().AnyAsync(c => c.Email == email, cancellationToken);

    public  async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) => _context.Customers.Add(customer);
    public void Update(Customer customer, CancellationToken cancellationToken = default) => _context.Customers.Update(customer);
    public void Delete(Customer customer, CancellationToken cancellationToken = default) => _context.Customers.Remove(customer);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
