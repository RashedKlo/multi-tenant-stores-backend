// Features/Support/Commands/RemoveMessage/RemoveMessageCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Support.Commands.RemoveMessage;

public sealed record RemoveMessageCommand(Guid MessageId) : IRequest<Result>;