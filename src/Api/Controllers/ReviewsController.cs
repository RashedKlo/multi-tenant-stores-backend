using Application.Features.Reviews.Commands.SubmitStoreReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/stores/{storeId:guid}/reviews")]
[Authorize]
public sealed class ReviewsController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Submits a review for a delivered order from the current customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Submit(
        [FromRoute] Guid storeId,
        [FromBody] SubmitStoreReviewRequest request,
        CancellationToken ct)
        => HandleResult(await mediator.Send(
            new SubmitStoreReviewCommand(storeId, request.OrderId, request.Rating, request.Comment), ct));
}

