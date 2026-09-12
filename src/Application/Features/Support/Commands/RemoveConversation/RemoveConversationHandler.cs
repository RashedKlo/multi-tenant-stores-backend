// RemoveConversationHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Commands.RemoveConversation;

public sealed class RemoveConversationHandler(
    ISupportConversationRepository conversations,
    ICurrentUserService user,
    ISupportChatNotifier notifier)
    : IRequestHandler<RemoveConversationCommand, Result>
{
    public async Task<Result> Handle(RemoveConversationCommand request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var conversation = await conversations.GetByIdForCustomerAsync(
            request.ConversationId, user.CustomerId.Value, cancellationToken);
        if (conversation is null)
            return Result.Failure(Error.NotFound("SupportConversation.NotFound", "Conversation not found."));

        var result = conversation.Delete();
        if (result.IsFailure)
            return result;

        await conversations.SaveChangesAsync(cancellationToken);

        await notifier.NotifyConversationDeletedAsync(conversation.TenantId, conversation.Id, cancellationToken);
        return Result.Success();
    }
}