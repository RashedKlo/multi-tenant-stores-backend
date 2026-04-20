using Application.Tenants.Commands.CreateTenant;
using Application.Tenants.Commands.DeleteTenant;
using Application.Tenants.Commands.UpdateTenant;
using Application.Tenants.Queries.GetAllTenants;
using Application.Tenants.Queries.GetTenantById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/tenants")]
[EnableRateLimiting("fixed")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all active Tenants ordered by Order field</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllTenantsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single Tenant by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery(id), cancellationToken);
        if (result is null) return NotFound($"Tenant with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Create a new Tenant</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing Tenant</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTenantCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in body.");

        var result = await _mediator.Send(command, cancellationToken);
        if (result is null) return NotFound($"Tenant with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Delete a Tenant</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteTenantCommand(id), cancellationToken);
        if (!deleted) return NotFound($"Tenant with ID {id} was not found.");
        return NoContent();
    }
}