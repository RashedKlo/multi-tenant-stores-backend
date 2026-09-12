// GetMyConversationsHandler.cs
using Application.Common.Interfaces;
using Application.Features.Support.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Queries.GetMyConversations;

public sealed class GetMyConversationsHandler(ISupportChatQueries queries, ICurrentUserService user)
    : IRequestHandler<GetMyConversationsQuery, Result<IReadOnlyList<ConversationSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> Handle(
        GetMyConversationsQuery request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<IReadOnlyList<ConversationSummaryDto>>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var items = await queries.GetConversationsForCustomerAsync(user.CustomerId.Value, cancellationToken);
        return Result<IReadOnlyList<ConversationSummaryDto>>.Success(items);
    }
}