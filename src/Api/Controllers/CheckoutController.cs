using Application.Checkout.DTOs;
using Application.Features.Checkout.Commands.Checkout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize]
[EnableRateLimiting("fixed")]
public sealed class CheckoutController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Places an order from the current cart and returns a Stripe Checkout URL.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CheckoutResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckoutResultDto>> Checkout(
        [FromBody] CheckoutCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));
}