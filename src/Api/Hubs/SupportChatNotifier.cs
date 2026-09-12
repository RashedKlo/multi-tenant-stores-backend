// src/Api/Hubs/SupportChatNotifier.cs
using Application.Common.Interfaces;
using Application.Features.Support.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public sealed class SupportChatNotifier(IHubContext<SupportChatHub> hub) : ISupportChatNotifier
{
    public Task NotifyNewMessageAsync(Guid recipientId, MessageDto message, CancellationToken cancellationToken = default)
        => hub.Clients.User(recipientId.ToString()).SendAsync("NewMessage", message, cancellationToken);

    public Task NotifyMessagesReadAsync(Guid recipientId, Guid conversationId, CancellationToken cancellationToken = default)
        => hub.Clients.User(recipientId.ToString()).SendAsync(
            "MessagesRead", new { conversationId, readAt = DateTime.UtcNow }, cancellationToken);

    public Task NotifyMessageDeletedAsync(
        Guid recipientId, Guid conversationId, Guid messageId, CancellationToken cancellationToken = default)
        => hub.Clients.User(recipientId.ToString()).SendAsync(
            "MessageDeleted", new { conversationId, messageId }, cancellationToken);

    public Task NotifyConversationDeletedAsync(
        Guid recipientId, Guid conversationId, CancellationToken cancellationToken = default)
        => hub.Clients.User(recipientId.ToString()).SendAsync(
            "ConversationDeleted", new { conversationId }, cancellationToken);
}