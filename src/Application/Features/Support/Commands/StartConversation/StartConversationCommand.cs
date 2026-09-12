// Features/Support/Commands/StartConversation/StartConversationCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Commands.StartConversation;

public sealed record StartConversationCommand(Guid TenantId) : IRequest<Result<Guid>>;