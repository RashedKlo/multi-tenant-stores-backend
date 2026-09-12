using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class SupportConversation
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public SupportConversationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastMessageAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public Tenant Tenant { get; private set; } = null!;
    public Customer Customer { get; private set; } = null!;

    private SupportConversation() { }

    public static Result<SupportConversation> Create(Guid tenantId, Guid customerId)
    {
        var errors = new List<Error>();
        DomainValidation.EnsureNotEmptyGuid(tenantId, errors, "TenantId");
        DomainValidation.EnsureNotEmptyGuid(customerId, errors, "CustomerId");

        if (errors.Count > 0)
            return Result<SupportConversation>.Failure(errors);

        var now = DateTime.UtcNow;

        var conversation = new SupportConversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            Status = SupportConversationStatus.Open,
            CreatedAt = now,
            LastMessageAt = now
        };

        return Result<SupportConversation>.Success(conversation);
    }

    /// <summary>
    /// Bumps last_message_at and reopens the conversation if it had been closed.
    /// Called by SendMessage before persisting.
    /// </summary>
    public Result RegisterNewMessage()
    {
        if (IsDeleted)
            return Result.Failure(Error.Conflict("SupportConversation.Deleted", "Conversation has been removed."));

        LastMessageAt = DateTime.UtcNow;
        Status = SupportConversationStatus.Open;
        return Result.Success();
    }

    public Result Close()
    {
        if (IsDeleted)
            return Result.Failure(Error.Conflict("SupportConversation.Deleted", "Conversation has been removed."));

        Status = SupportConversationStatus.Closed;
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return Result.Success();

        DeletedAt = DateTime.UtcNow;
        return Result.Success();
    }
}