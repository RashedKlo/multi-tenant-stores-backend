// Features/Support/Queries/GetMyConversations/GetMyConversationsQuery.cs
using Application.Features.Support.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Queries.GetMyConversations;

public sealed record GetMyConversationsQuery : IRequest<Result<IReadOnlyList<ConversationSummaryDto>>>;