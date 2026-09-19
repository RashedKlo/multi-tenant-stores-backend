// StartConversationHandler.cs — idempotent: reuses an existing open conversation instead
// of creating duplicates if the customer messages the same tenant again.
using Application.Common.Interfaces;
using Application.Features.Support.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Commands.StartConversation;

public sealed class StartConversationHandler(
    ISupportConversationRepository conversations,
    ICurrentUserService user)
    : IRequestHandler<StartConversationCommand, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<ConversationDto>.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var existing = await conversations.GetOpenByCustomerAndTenantAsync(
            user.CustomerId.Value, request.TenantId, cancellationToken);
        if (existing is not null)
            return Result<ConversationDto>.Success(ConversationDto.FromEntity(existing));

        var result = SupportConversation.Create(request.TenantId, user.CustomerId.Value);
        if (result.IsFailure)
            return Result<ConversationDto>.Failure(result.Errors);

        await conversations.AddAsync(result.Value!, cancellationToken);
        await conversations.SaveChangesAsync(cancellationToken);

        return Result<ConversationDto>.Success(ConversationDto.FromEntity(result.Value!));
    }
}