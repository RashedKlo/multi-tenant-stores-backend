// src/Infrastructure/Persistence/Repositories/SupportMessageRepository.cs
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SupportMessageRepository(AppDbContext context) : ISupportMessageRepository
{
    public Task<SupportMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.SupportMessages
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);

    public Task<List<SupportMessage>> GetUnreadNotSentByAsync(
        Guid conversationId, Guid senderId, CancellationToken cancellationToken = default) =>
        context.SupportMessages
            .Where(m => m.ConversationId == conversationId
                     && m.DeletedAt == null
                     && !m.IsRead
                     && m.SenderId != senderId)
            .ToListAsync(cancellationToken);

    public void Add(SupportMessage message) => context.SupportMessages.Add(message);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}