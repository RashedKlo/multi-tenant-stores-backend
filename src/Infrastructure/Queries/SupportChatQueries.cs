// src/Infrastructure/Queries/SupportChatQueries.cs
using Application.Common.Interfaces;
using Application.Features.Support.DTOs;
using Dapper;
using Infrastructure.Persistence;

namespace Infrastructure.Queries;

public sealed class SupportChatQueries(IDbConnectionFactory connectionFactory) : ISupportChatQueries
{
  private sealed class SupportConversationRow
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public string Status { get; set; } = "";
    public string? LastMessageBody { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsForCustomerAsync(
    Guid customerId,
    CancellationToken cancellationToken = default)
{
    const string sql = """
        SELECT
            c.id              AS "Id",
            c.tenant_id       AS "TenantId",
            t.name            AS "TenantName",
            c.status::text    AS "Status",
            lm.body           AS "LastMessageBody",
            c.last_message_at AS "LastMessageAt",
            COALESCE(unread.count, 0)::int AS "UnreadCount"
        FROM support_conversations c
        JOIN tenants t ON t.id = c.tenant_id
        LEFT JOIN LATERAL (
            SELECT body
            FROM support_messages m
            WHERE m.conversation_id = c.id AND m.deleted_at IS NULL
            ORDER BY m.created_at DESC
            LIMIT 1
        ) lm ON true
        LEFT JOIN LATERAL (
            SELECT COUNT(*) AS count
            FROM support_messages m
            WHERE m.conversation_id = c.id
              AND m.deleted_at IS NULL
              AND m.is_read = false
              AND m.sender_id <> @CustomerId
        ) unread ON true
        WHERE c.customer_id = @CustomerId
          AND c.deleted_at IS NULL
        ORDER BY c.last_message_at DESC
        """;

    using var connection = connectionFactory.CreateConnection();
    var rows = await connection.QueryAsync<SupportConversationRow>(
        new CommandDefinition(sql, new { CustomerId = customerId }, cancellationToken: cancellationToken));

    return rows
        .Select(r => new ConversationSummaryDto(
            r.Id,
            r.TenantId,
            r.TenantName,
            r.Status,
            r.LastMessageBody,
            r.LastMessageAt,
            r.UnreadCount))
        .ToList();
}
    public async Task<(IReadOnlyList<MessageDto> Items, int TotalCount)> GetMessagesAsync(
        Guid conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        const string countSql = """
            SELECT COUNT(*) FROM support_messages
            WHERE conversation_id = @ConversationId AND deleted_at IS NULL
            """;

        const string pageSql = """
            SELECT
                id                 AS Id,
                conversation_id    AS ConversationId,
                sender_type::text  AS SenderType,
                sender_id          AS SenderId,
                body               AS Body,
                is_read            AS IsRead,
                created_at         AS CreatedAt
            FROM support_messages
            WHERE conversation_id = @ConversationId AND deleted_at IS NULL
            ORDER BY created_at DESC
            OFFSET @Offset LIMIT @PageSize
            """;

        using var connection = connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, new { ConversationId = conversationId }, cancellationToken: cancellationToken));

        var items = await connection.QueryAsync<MessageDto>(
            new CommandDefinition(pageSql, new
            {
                ConversationId = conversationId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            }, cancellationToken: cancellationToken));

        return (items.ToList(), totalCount);
    }
}