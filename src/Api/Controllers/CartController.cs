using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Cart.DTOs;
using Application.Features.Cart.Commands.AddCartItem;      
using Application.Features.Cart.Commands.UpdateCartItem;
using Application.Features.Cart.Commands.RemoveCartItem;
using Application.Features.Cart.Commands.ClearCart;
using Application.Features.Cart.Queries.GetCartItems;
using Api.Requests.Cart;

namespace Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Returns the current cart items for the authenticated customer or guest session.
    /// Always succeeds — returns an empty list when no cart exists.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CartItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CartItemDto>>> GetCart(CancellationToken ct)
        => HandleResult(await mediator.Send(new GetCartItemsQuery(), ct));

    /// <summary>
    /// Adds an item to the cart (creates the cart if it does not exist).
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddItem(
        [FromBody] AddCartItemRequest command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new AddCartItemCommand(command.StoreId, command.ProductId, command.Quantity, command.Notes, command.OptionIds), ct));

    /// <summary>
    /// Updates the quantity of an existing cart item.
    /// Identity comes exclusively from the route. Any CartItemId in the body is ignored.
    /// </summary>
    [HttpPut("items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateItem(
       [FromRoute] Guid id,
        [FromBody] UpdateCartItemRequest command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new UpdateCartItemCommand(id, command.StoreId, command.Quantity), ct));

    /// <summary>
    /// Removes a single item from the cart.
    /// </summary>
    [HttpDelete("items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveItem(
       [FromRoute] Guid id,
        [FromQuery] Guid storeId,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new RemoveCartItemCommand(id, storeId), ct));

    /// <summary>
    /// Clears all items from the cart of the given store.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ClearCart(
        [FromQuery] Guid storeId,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new ClearCartCommand(storeId), ct));
}