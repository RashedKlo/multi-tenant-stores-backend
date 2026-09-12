// Features/Support/Commands/SendMessage/SendMessageCommand.cs
using Application.Features.Support.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Commands.SendMessage;

public sealed record SendMessageCommand(Guid ConversationId, string Body) : IRequest<Result<MessageDto>>;