// src/Application/Common/Interfaces/ISupportChatQueries.cs
using Application.Features.Support.DTOs;

namespace Application.Common.Interfaces;

public interface ISupportChatQueries
{
    Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsForCustomerAsync(
        Guid customerId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<MessageDto> Items, int TotalCount)> GetMessagesAsync(
        Guid conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}