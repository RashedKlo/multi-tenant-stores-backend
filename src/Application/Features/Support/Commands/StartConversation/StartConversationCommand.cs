// Features/Support/Commands/StartConversation/StartConversationCommand.cs
using Application.Features.Support.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Commands.StartConversation;

public sealed record StartConversationCommand(Guid TenantId) : IRequest<Result<ConversationDto>>;