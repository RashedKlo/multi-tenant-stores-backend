using Domain.Entities;

namespace Domain.Interfaces;

public interface ISupportMessageRepository
{
    Task<SupportMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Unread messages in a conversation that weren't sent by senderId.</summary>
    Task<List<SupportMessage>> GetUnreadNotSentByAsync(
        Guid conversationId, Guid senderId, CancellationToken cancellationToken = default);

    void Add(SupportMessage message);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}