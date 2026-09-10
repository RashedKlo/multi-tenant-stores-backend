using Api.Requests.Address;
using Application.Addresses.Commands.CreateAddress;
using Application.Addresses.Commands.DeleteAddress;
using Application.Addresses.Commands.SetDefaultAddress;
using Application.Addresses.Commands.UpdateAddress;
using Application.Addresses.DTOs;
using Application.Addresses.Queries.GetAddressById;
using Application.Addresses.Queries.GetAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
[EnableRateLimiting("fixed")]
public class AddressesController(IMediator mediator) : ApiControllerBase
{
    /// <summary>
    /// Returns all addresses of the current customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAddresses(CancellationToken ct)
        => HandleResult(await mediator.Send(new GetAddressesQuery(), ct));

    /// <summary>
    /// Returns a single address by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetAddressByIdQuery(id), ct));

    /// <summary>
    /// Creates a new address for the current customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddressDto>> Create(
        [FromBody] AddAddressRequest request,
        CancellationToken ct)
    {
        var command = new CreateAddressCommand(
            request.Label,
            request.Latitude,
            request.Longitude,
            request.AddressText,
            request.IsDefault);

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return HandleFailure(result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates an existing address.
    /// Identity comes exclusively from the route.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAddressRequest request,
        CancellationToken ct)
    {
        var command = new UpdateAddressCommand(
            id,
            request.Label,
            request.Latitude,
            request.Longitude,
            request.AddressText);

        return HandleResult(await mediator.Send(command, ct));
    }

    /// <summary>
    /// Soft-deletes an address.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new DeleteAddressCommand(id), ct));

    /// <summary>
    /// Sets the given address as the default one (unsets previous default).
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> SetDefault(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new SetDefaultAddressCommand(id), ct));
}