// RemoveMessageHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Commands.RemoveMessage;

public sealed class RemoveMessageHandler(
    ISupportMessageRepository messages,
    ISupportConversationRepository conversations,
    ICurrentUserService user,
    ISupportChatNotifier notifier)
    : IRequestHandler<RemoveMessageCommand, Result>
{
    public async Task<Result> Handle(RemoveMessageCommand request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var message = await messages.GetByIdAsync(request.MessageId, cancellationToken);
        if (message is null)
            return Result.Failure(Error.NotFound("SupportMessage.NotFound", "Message not found."));

        var result = message.Delete(user.CustomerId.Value); // ownership check lives in the domain method
        if (result.IsFailure)
            return result;

        await messages.SaveChangesAsync(cancellationToken);

        var conversation = await conversations.GetByIdAsync(message.ConversationId, cancellationToken);
        if (conversation is not null)
            await notifier.NotifyMessageDeletedAsync(conversation.TenantId, conversation.Id, message.Id, cancellationToken);

        return Result.Success();
    }
}