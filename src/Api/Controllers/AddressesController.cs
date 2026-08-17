using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Addresses.Commands.CreateAddress;
using Application.Addresses.Commands.DeleteAddress;
using Application.Addresses.Commands.SetDefaultAddress;
using Application.Addresses.Commands.UpdateAddress;
using Application.Addresses.Queries.GetAddressById;
using Application.Addresses.Queries.GetAddresses;
using Application.Addresses.DTOs;
// ============================================================
// 2. AddressesController
// ============================================================
[ApiController]
[Route("api")]
public class AddressesController(IMediator mediator) : ControllerBase
{
    [HttpGet("addresses")]
    public async Task<ActionResult<List<AddressDto>>> GetAddresses(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAddressesQuery(), ct));

    [HttpPost("addresses")]
    public async Task<ActionResult<AddressDto>> Create(
        [FromBody] CreateAddressCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpGet("addresses/{id:guid}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetAddressByIdQuery(id), ct));

    [HttpPut("addresses/{id:guid}")]
    public async Task<ActionResult<AddressDto>> Update(
        Guid id, [FromBody] UpdateAddressCommand command, CancellationToken ct)
    {
        // ensure route id wins over any id in body
        command = command with { Id = id };
        return Ok(await mediator.Send(command, ct));
    }

    [HttpDelete("addresses/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteAddressCommand(id), ct);
        return NoContent();
    }

    [HttpPost("addresses/{id:guid}/set-default")]
    public async Task<ActionResult> SetDefault(Guid id, CancellationToken ct)
    {
        await mediator.Send(new SetDefaultAddressCommand(id), ct);
        return NoContent();
    }
}