// Features/Support/Commands/RemoveConversation/RemoveConversationCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Commands.RemoveConversation;

public sealed record RemoveConversationCommand(Guid ConversationId) : IRequest<Result>;