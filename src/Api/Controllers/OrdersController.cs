using MediatR;
using Microsoft.AspNetCore.Mvc;
// ============================================================
// 7. OrdersController  (Checkout + Orders + Stripe webhook)
// ============================================================
[ApiController]
[Route("api")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResultDto>> Checkout(
        [FromBody] CheckoutCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    // Stripe sends the raw body; your handler should read it from HttpContext if needed.
    // Keep the command thin — the webhook handler can take the event id / payload.
    [HttpPost("webhooks/stripe")]
    public async Task<ActionResult> StripeWebhook(
        [FromBody] HandleStripeWebhookCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok();
    }

    [HttpGet("orders")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetOrders(
        string? status,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await mediator.Send(new GetOrdersQuery(status, page, pageSize), ct));

    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetOrderByIdQuery(id), ct));
}