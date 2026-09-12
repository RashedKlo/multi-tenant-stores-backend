// src/Infrastructure/Persistence/Repositories/SupportConversationRepository.cs
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SupportConversationRepository(AppDbContext context) : ISupportConversationRepository
{
    public Task<SupportConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.SupportConversations
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);

    public Task<SupportConversation?> GetByIdForCustomerAsync(
        Guid id, Guid customerId, CancellationToken cancellationToken = default) =>
        context.SupportConversations
            .FirstOrDefaultAsync(
                c => c.Id == id && c.CustomerId == customerId && c.DeletedAt == null, cancellationToken);

    public Task<SupportConversation?> GetOpenByCustomerAndTenantAsync(
        Guid customerId, Guid tenantId, CancellationToken cancellationToken = default) =>
        context.SupportConversations
            .FirstOrDefaultAsync(
                c => c.CustomerId == customerId && c.TenantId == tenantId && c.DeletedAt == null,
                cancellationToken);

    public void Add(SupportConversation conversation) => context.SupportConversations.Add(conversation);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}