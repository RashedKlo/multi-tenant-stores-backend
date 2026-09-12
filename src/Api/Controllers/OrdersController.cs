using Application.Common.Models;
using Application.Features.Orders.Commands.ChangeOrderStatus;
using Application.Features.Orders.DTOs;
using Application.Features.Orders.Queries.GetOrderById;
using Application.Features.Orders.Queries.GetOrders;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
[EnableRateLimiting("fixed")]
public sealed class OrdersController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Returns a paged list of the current customer's orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(new GetOrdersQuery(status, page, pageSize), ct));

    /// <summary>
    /// Returns a single order owned by the current customer.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetOrderByIdQuery(id), ct));

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangeStatus(
    [FromRoute] Guid id,
    [FromBody] ChangeOrderStatusRequest request,
    CancellationToken ct)
    => HandleResult(await mediator.Send(
        new ChangeOrderStatusCommand(id, request.NewStatus, request.Note), ct));
}
