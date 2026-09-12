// StartConversationHandler.cs — idempotent: reuses an existing open conversation instead
// of creating duplicates if the customer messages the same tenant again.
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Support.Commands.StartConversation;

public sealed class StartConversationHandler(
    ISupportConversationRepository conversations,
    ICurrentUserService user)
    : IRequestHandler<StartConversationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || user.CustomerId is null)
            return Result<Guid>.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var existing = await conversations.GetOpenByCustomerAndTenantAsync(
            user.CustomerId.Value, request.TenantId, cancellationToken);
        if (existing is not null)
            return Result<Guid>.Success(existing.Id);

        var result = SupportConversation.Create(request.TenantId, user.CustomerId.Value);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Errors);

        conversations.Add(result.Value!);
        await conversations.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(result.Value!.Id);
    }
}