// SendMessageHandler.cs — persist first, then notify. Same order as ChangeOrderStatusHandler.
using Application.Common.Interfaces;
using Application.Features.Support.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Commands.SendMessage;

public sealed class SendMessageHandler(
    ISupportConversationRepository conversations,
    ISupportMessageRepository messages,
    ICurrentUserService user,
    ISupportChatNotifier notifier)
    : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<MessageDto>.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var conversation = await conversations.GetByIdForCustomerAsync(
            request.ConversationId, user.CustomerId.Value, cancellationToken);
        if (conversation is null)
            return Result<MessageDto>.Failure(Error.NotFound("SupportConversation.NotFound", "Conversation not found."));

        var messageResult = SupportMessage.Create(
            conversation.Id, SupportSenderType.Customer, user.CustomerId.Value, request.Body);
        if (messageResult.IsFailure)
            return Result<MessageDto>.Failure(messageResult.Errors);

        var touchResult = conversation.RegisterNewMessage();
        if (touchResult.IsFailure)
            return Result<MessageDto>.Failure(touchResult.Errors);

        messages.Add(messageResult.Value!);
        await messages.SaveChangesAsync(cancellationToken); // also flushes conversation.LastMessageAt (same DbContext)

        var dto = MessageDto.FromEntity(messageResult.Value!);
        await notifier.NotifyNewMessageAsync(conversation.TenantId, dto, cancellationToken);

        return Result<MessageDto>.Success(dto);
    }
}