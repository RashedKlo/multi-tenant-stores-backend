using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class SupportMessage
{
    private const int MaxBodyLength = 2000;

    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public SupportSenderType SenderType { get; private set; }
    public Guid SenderId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public SupportConversation Conversation { get; private set; } = null!;

    private SupportMessage() { }

    public static Result<SupportMessage> Create(
        Guid conversationId, SupportSenderType senderType, Guid senderId, string body)
    {
        var errors = new List<Error>();
        DomainValidation.EnsureNotEmptyGuid(conversationId, errors, "ConversationId");
        DomainValidation.EnsureNotEmptyGuid(senderId, errors, "SenderId");

        body = DomainValidation.NormalizeRequiredString(body, errors, "Body");
        if (body.Length > MaxBodyLength)
            errors.Add(Error.Validation(
                "SupportMessage.Body.TooLong", $"Message cannot exceed {MaxBodyLength} characters."));

        if (errors.Count > 0)
            return Result<SupportMessage>.Failure(errors);

        var message = new SupportMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderType = senderType,
            SenderId = senderId,
            Body = body,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        return Result<SupportMessage>.Success(message);
    }

    public Result MarkAsRead()
    {
        if (IsDeleted)
            return Result.Failure(Error.Conflict("SupportMessage.Deleted", "Message has been removed."));

        IsRead = true;
        return Result.Success();
    }

    /// <summary>
    /// Only the customer who sent a Customer-type message may remove it.
    /// System messages can't be removed through this path.
    /// </summary>
    public Result Delete(Guid requestedBy)
    {
        if (IsDeleted)
            return Result.Success();

        if (SenderType != SupportSenderType.Customer || SenderId != requestedBy)
            return Result.Failure(Error.Forbidden(
                "SupportMessage.NotOwner", "You can only remove your own messages."));

        DeletedAt = DateTime.UtcNow;
        return Result.Success();
    }
}