// Api/Controllers/CheckoutController.cs
using Application.Features.Checkout.Commands.Checkout;
using Application.Checkout.DTOs;
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
    /// Creates a Pending order + Stripe Checkout Session and returns the hosted URL.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CheckoutResultDto>> Checkout(
        [FromBody] CheckoutCommand command,
        CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));
}