// Domain/Interfaces/ICustomerAddressRepository.cs
using Domain.Entities;

namespace Domain.Interfaces;

public interface ICustomerAddressRepository
{
    Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CustomerAddress?> GetByIdForCustomerAsync(
        Guid id, Guid customerId, CancellationToken ct = default);

    Task<CustomerAddress?> GetDefaultForCustomerAsync(
        Guid customerId, CancellationToken ct = default);

    Task<IReadOnlyList<CustomerAddress>> GetActiveByCustomerAsync(
        Guid customerId, CancellationToken ct = default);

    Task AddAsync(CustomerAddress address, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}