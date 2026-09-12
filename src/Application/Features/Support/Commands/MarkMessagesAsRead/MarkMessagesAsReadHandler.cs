// Features/Support/Commands/MarkMessagesAsRead/MarkMessagesAsReadCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Commands.MarkMessagesAsRead;

public sealed record MarkMessagesAsReadCommand(Guid ConversationId) : IRequest<Result>;