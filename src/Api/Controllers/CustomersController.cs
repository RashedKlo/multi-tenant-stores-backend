using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Customers.Commands.UpdateProfile;
using Application.Customers.Commands.ChangePassword;
using Application.Customers.DTOs;

namespace Api.Controllers;

// Settings / me
[ApiController]
[Route("api")]
public class CustomersController(IMediator mediator) : ApiControllerBase
{
    [HttpPut("customers/me")]
    public async Task<ActionResult<CustomerDto>> UpdateProfile(
        [FromBody] UpdateProfileCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPut("customers/me/password")]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));
}
