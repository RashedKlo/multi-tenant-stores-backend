using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Addresses.Commands.CreateAddress;
using Application.Addresses.Commands.DeleteAddress;
using Application.Addresses.Commands.SetDefaultAddress;
using Application.Addresses.Commands.UpdateAddress;
using Application.Addresses.Queries.GetAddressById;
using Application.Addresses.Queries.GetAddresses;
using Application.Addresses.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Authorize]
[Route("api/addresses")]
public class AddressesController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AddressDto>>> GetAddresses(CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetAddressesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetAddressByIdQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(
        [FromBody] CreateAddressCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return HandleFailure(result);

        // RESTful: 201 Created with Location header pointing at the new resource
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AddressDto>> Update(
        Guid id, [FromBody] UpdateAddressCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<bool>> Delete(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new DeleteAddressCommand(id), ct));

    [HttpPost("{id:guid}/set-default")]
    public async Task<ActionResult<AddressDto>> SetDefault(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new SetDefaultAddressCommand(id), ct));
}
