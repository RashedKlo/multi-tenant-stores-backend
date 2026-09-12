// src/Application/Common/Interfaces/ISupportChatNotifier.cs
using Application.Features.Support.DTOs;

namespace Application.Common.Interfaces;

/// <summary>
/// Pushes real-time support-chat events to whichever side (customer or tenant)
/// isn't the sender. Implemented in Api with SignalR — Application only knows this interface.
/// </summary>
public interface ISupportChatNotifier
{
    Task NotifyNewMessageAsync(Guid recipientId, MessageDto message, CancellationToken cancellationToken = default);

    Task NotifyMessagesReadAsync(Guid recipientId, Guid conversationId, CancellationToken cancellationToken = default);

    Task NotifyMessageDeletedAsync(
        Guid recipientId, Guid conversationId, Guid messageId, CancellationToken cancellationToken = default);

    Task NotifyConversationDeletedAsync(
        Guid recipientId, Guid conversationId, CancellationToken cancellationToken = default);
}