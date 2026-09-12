// ConversationSummaryDto.cs
namespace Application.Features.Support.DTOs;

public sealed record ConversationSummaryDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string Status,
    string? LastMessageBody,
    DateTime LastMessageAt,
    int UnreadCount);