using Domain.Entities;

namespace Application.Features.Support.DTOs;

public sealed record ConversationDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    Guid CustomerId,
    string Status,
    DateTime CreatedAt,
    DateTime LastMessageAt)
{
    public static ConversationDto FromEntity(SupportConversation conversation) => new(
        conversation.Id,
        conversation.TenantId,
        conversation.Tenant?.Name ?? string.Empty,
        conversation.CustomerId,
        conversation.Status.ToString(),
        conversation.CreatedAt,
        conversation.LastMessageAt);
}
