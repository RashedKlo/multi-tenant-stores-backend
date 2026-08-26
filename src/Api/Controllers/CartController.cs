using Application.Carts.Queries.GetCartItems;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("fixed")]
public class CartController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Get current cart for the authenticated customer or guest session.
    /// Always returns a CartDto (empty cart if none exists yet) — this query never fails.
    /// </summary>
    [HttpGet("cart")]
    public async Task<ActionResult<CartDto>> GetCart(
        [FromQuery] Guid storeId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetCartItemsQuery(storeId), ct));

    [HttpPost("cart/items")]
    public async Task<ActionResult> AddItem(
        [FromBody] AddCartItemCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPut("cart/items/{id:guid}")]
    public async Task<ActionResult> UpdateItem(
        Guid id, [FromBody] UpdateCartItemCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command with { CartItemId = id }, ct));

    [HttpDelete("cart/items/{id:guid}")]
    public async Task<ActionResult> RemoveItem(
        Guid id, [FromQuery] Guid storeId, CancellationToken ct) =>
        HandleResult(await mediator.Send(new RemoveCartItemCommand(id, storeId), ct));

    [HttpDelete("cart")]
    public async Task<ActionResult> ClearCart(
        [FromQuery] Guid storeId, CancellationToken ct) =>
        HandleResult(await mediator.Send(new ClearCartCommand(storeId), ct));
}
