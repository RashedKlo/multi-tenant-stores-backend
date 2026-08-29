// Api/Controllers/WebhooksController.cs
using Application.Features.Checkout.Commands.HandleStripeWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public sealed class WebhooksController(IMediator mediator) : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> Stripe(CancellationToken ct)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync(ct);

        var signature = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
            return BadRequest();

        var result = await mediator.Send(
            new HandleStripeWebhookCommand(json, signature), ct);

        if (result.IsFailure &&
            result.Errors.Any(e => e.Code == "Stripe.InvalidSignature"))
            return BadRequest();

        return Ok();
    }
}