// MessageDto.cs
using Domain.Entities;

namespace Application.Features.Support.DTOs;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    string SenderType,
    Guid SenderId,
    string Body,
    bool IsRead,
    DateTime CreatedAt)
{
    public static MessageDto FromEntity(SupportMessage message) => new(
        message.Id,
        message.ConversationId,
        message.SenderType.ToString(),
        message.SenderId,
        message.Body,
        message.IsRead,
        message.CreatedAt);
}