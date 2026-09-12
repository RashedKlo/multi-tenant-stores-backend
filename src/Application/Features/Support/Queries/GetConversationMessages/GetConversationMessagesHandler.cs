// GetConversationMessagesHandler.cs
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Support.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Queries.GetConversationMessages;

public sealed class GetConversationMessagesHandler(
    ISupportConversationRepository conversations,
    ISupportChatQueries queries,
    ICurrentUserService user)
    : IRequestHandler<GetConversationMessagesQuery, Result<PagedResult<MessageDto>>>
{
    public async Task<Result<PagedResult<MessageDto>>> Handle(
        GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<PagedResult<MessageDto>>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var conversation = await conversations.GetByIdForCustomerAsync(
            request.ConversationId, user.CustomerId.Value, cancellationToken);
        if (conversation is null)
            return Result<PagedResult<MessageDto>>.Failure(
                Error.NotFound("SupportConversation.NotFound", "Conversation not found."));

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 30 : request.PageSize;

        var (items, totalCount) = await queries.GetMessagesAsync(conversation.Id, page, pageSize, cancellationToken);
        return Result<PagedResult<MessageDto>>.Success(PagedResult<MessageDto>.Create(items, page, pageSize, totalCount));
    }
}