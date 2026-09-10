
using Application.Customers.Commands.ChangePassword;
using Application.Customers.Commands.UpdateProfile;
using Application.Customers.DTOs;
using Application.Customers.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
[EnableRateLimiting("fixed")]
public class CustomersController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Returns the profile of the currently authenticated customer.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDto>> GetMe(CancellationToken ct)
        => HandleResult(await mediator.Send(new GetMeQuery(), ct));

    /// <summary>
    /// Updates the profile of the currently authenticated customer.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> UpdateProfile(
        [FromBody] UpdateProfileCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    /// <summary>
    /// Changes the password of the currently authenticated customer.
    /// </summary>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));
}