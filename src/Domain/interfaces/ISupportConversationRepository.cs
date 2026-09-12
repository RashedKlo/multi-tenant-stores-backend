using Domain.Entities;

namespace Domain.Interfaces;

public interface ISupportConversationRepository
{
    Task<SupportConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SupportConversation?> GetByIdForCustomerAsync(
        Guid id, Guid customerId, CancellationToken cancellationToken = default);

    Task<SupportConversation?> GetOpenByCustomerAndTenantAsync(
        Guid customerId, Guid tenantId, CancellationToken cancellationToken = default);

    void Add(SupportConversation conversation);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}