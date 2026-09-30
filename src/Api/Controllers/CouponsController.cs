using Application.Features.Coupons.Commands.ApplyCoupon;
using Application.Features.Coupons.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/stores/{storeId:guid}/coupons")]
[Authorize]
public sealed class CouponsController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Validates a coupon against the current customer's cart.
    /// </summary>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(CouponApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponApplicationDto>> Apply(
        [FromRoute] Guid storeId,
        [FromBody] string Code,
        CancellationToken ct)
        => HandleResult(await mediator.Send(
            new ApplyCouponCommand(storeId,Code), ct));
}
