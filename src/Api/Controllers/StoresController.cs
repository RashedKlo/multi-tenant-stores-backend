using Application.Stores.Commands.CreateStore;
using Application.Stores.Commands.DeleteStore;
using Application.Stores.Commands.UpdateStore;
using Application.Stores.Queries.GetAllStores;
using Application.Stores.Queries.GetStoreById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/stores")]
[EnableRateLimiting("fixed")]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public StoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all active Stores ordered by Order field</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllStoresQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single Store by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoreByIdQuery(id), cancellationToken);
        if (result is null) return NotFound($"Store with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Create a new Store</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStoreCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing Store</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateStoreCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in body.");

        var result = await _mediator.Send(command, cancellationToken);
        if (result is null) return NotFound($"Store with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Delete a Store</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteStoreCommand(id), cancellationToken);
        if (!deleted) return NotFound($"Store with ID {id} was not found.");
        return NoContent();
    }
}