// MarkMessagesAsReadHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Commands.MarkMessagesAsRead;

public sealed class MarkMessagesAsReadHandler(
    ISupportConversationRepository conversations,
    ISupportMessageRepository messages,
    ICurrentUserService user,
    ISupportChatNotifier notifier)
    : IRequestHandler<MarkMessagesAsReadCommand, Result>
{
    public async Task<Result> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var conversation = await conversations.GetByIdForCustomerAsync(
            request.ConversationId, user.CustomerId.Value, cancellationToken);
        if (conversation is null)
            return Result.Failure(Error.NotFound("SupportConversation.NotFound", "Conversation not found."));

        var unread = await messages.GetUnreadNotSentByAsync(conversation.Id, user.CustomerId.Value, cancellationToken);
        if (unread.Count == 0)
            return Result.Success();

        foreach (var message in unread)
        {
            var result = message.MarkAsRead();
            if (result.IsFailure)
                return result;
        }

        await messages.SaveChangesAsync(cancellationToken);

        await notifier.NotifyMessagesReadAsync(conversation.TenantId, conversation.Id, cancellationToken);
        return Result.Success();
    }
}