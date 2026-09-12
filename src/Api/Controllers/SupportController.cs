// src/Api/Controllers/SupportController.cs
using Application.Common.Models;
using Application.Features.Support.Commands.MarkMessagesAsRead;
using Application.Features.Support.Commands.RemoveConversation;
using Application.Features.Support.Commands.RemoveMessage;
using Application.Features.Support.Commands.SendMessage;
using Application.Features.Support.Commands.StartConversation;
using Application.Features.Support.DTOs;
using Application.Features.Support.Queries.GetConversationMessages;
using Application.Features.Support.Queries.GetMyConversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/support/conversations")]
[Authorize]
[EnableRateLimiting("fixed")]
public sealed class SupportController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConversationSummaryDto>>> GetMyConversations(CancellationToken ct)
        => HandleResult(await mediator.Send(new GetMyConversationsQuery(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> Start([FromBody] StartConversationRequest request, CancellationToken ct)
        => HandleResult(await mediator.Send(new StartConversationCommand(request.TenantId), ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remove([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await mediator.Send(new RemoveConversationCommand(id), ct));

    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(PagedResult<MessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetMessages(
        [FromRoute] Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default)
        => HandleResult(await mediator.Send(new GetConversationMessagesQuery(id, page, pageSize), ct));

    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageDto>> SendMessage(
        [FromRoute] Guid id, [FromBody] SendMessageRequest request, CancellationToken ct)
        => HandleResult(await mediator.Send(new SendMessageCommand(id, request.Body), ct));

    [HttpDelete("messages/{messageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RemoveMessage([FromRoute] Guid messageId, CancellationToken ct)
        => HandleResult(await mediator.Send(new RemoveMessageCommand(messageId), ct));

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> MarkAsRead([FromRoute] Guid id, CancellationToken ct)
        => HandleResult(await mediator.Send(new MarkMessagesAsReadCommand(id), ct));
}

public sealed record StartConversationRequest(Guid TenantId);
public sealed record SendMessageRequest(string Body);