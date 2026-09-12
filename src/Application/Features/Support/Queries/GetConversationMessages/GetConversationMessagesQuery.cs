// Features/Support/Queries/GetConversationMessages/GetConversationMessagesQuery.cs
using Application.Common.Models;
using Application.Features.Support.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(
    Guid ConversationId, int Page = 1, int PageSize = 30) : IRequest<Result<PagedResult<MessageDto>>>;