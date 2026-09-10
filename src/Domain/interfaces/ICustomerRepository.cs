// Domain/Interfaces/ICustomerRepository.cs
using Domain.Entities;

namespace Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    Task AddAsync(Customer customer, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}