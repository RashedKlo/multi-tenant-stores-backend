
using Application.Carts.Queries.GetCartItems;
using Application.Common.Models;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("fixed")]
public class CartController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current cart for the authenticated customer or guest session
    /// </summary>
    [HttpGet("cart")]
    public async Task<ActionResult<CartDto>> GetCart(
        [FromQuery] Guid storeId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCartItemsQuery(storeId), cancellationToken);
        return Ok(result); // GetCart always returns CartDto (empty if not found)
    }

    /// <summary>
    /// Add an item to the cart
    /// </summary>
    [HttpPost("cart/items")]
    public async Task<ActionResult> AddItem(
        [FromBody] AddCartItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : HandleFailure(result);
    }

    /// <summary>
    /// Update quantity of an existing cart item
    /// </summary>
    [HttpPut("cart/items/{id:guid}")]
    public async Task<ActionResult> UpdateItem(
        Guid id,
        [FromBody] UpdateCartItemCommand command,
        CancellationToken cancellationToken)
    {
        // Ensure the route id is used
        command = command with { CartItemId = id };

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : HandleFailure(result);
    }

    /// <summary>
    /// Remove a specific item from the cart
    /// </summary>
    [HttpDelete("cart/items/{id:guid}")]
    public async Task<ActionResult> RemoveItem(
        Guid id,
        [FromQuery] Guid storeId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveCartItemCommand(id, storeId),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : HandleFailure(result);
    }

    /// <summary>
    /// Clear all items from the cart
    /// </summary>
    [HttpDelete("cart")]
    public async Task<ActionResult> ClearCart(
        [FromQuery] Guid storeId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ClearCartCommand(storeId),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : HandleFailure(result);
    }
}