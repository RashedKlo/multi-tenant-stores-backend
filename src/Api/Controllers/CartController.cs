using MediatR;
using Microsoft.AspNetCore.Mvc;
// ============================================================
// 6. CartController
// ============================================================
[ApiController]
[Route("api")]
public class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet("cart")]
    public async Task<ActionResult<CartDto>> GetCart(
        [FromQuery] Guid storeId, CancellationToken ct) =>
        Ok(await mediator.Send(new GetCartQuery(storeId), ct));

    [HttpPost("cart/items")]
    public async Task<ActionResult<CartItemDto>> AddItem(
        [FromBody] AddCartItemCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPut("cart/items/{id:guid}")]
    public async Task<ActionResult<CartItemDto>> UpdateItem(
        Guid id, [FromBody] UpdateCartItemCommand command, CancellationToken ct)
    {
        command = command with { Id = id };
        return Ok(await mediator.Send(command, ct));
    }

    [HttpDelete("cart/items/{id:guid}")]
    public async Task<ActionResult> RemoveItem(Guid id, CancellationToken ct)
    {
        await mediator.Send(new RemoveCartItemCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("cart")]
    public async Task<ActionResult> ClearCart(
        [FromQuery] Guid storeId, CancellationToken ct)
    {
        await mediator.Send(new ClearCartCommand(storeId), ct);
        return NoContent();
    }
}