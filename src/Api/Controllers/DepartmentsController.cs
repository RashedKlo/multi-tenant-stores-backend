using Application.Departments.Commands.CreateDepartment;
using Application.Departments.Commands.DeleteDepartment;
using Application.Departments.Commands.UpdateDepartment;
using Application.Departments.Queries.GetAllDepartments;
using Application.Departments.Queries.GetDepartmentById;
using Application.Departments.Queries.GetDepartmentsByStoreId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/departments")]
[EnableRateLimiting("fixed")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all departments</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllDepartmentsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single department by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id), cancellationToken);
        if (result is null) return NotFound($"Department with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Get all departments by Store ID</summary>
    [HttpGet("store/{storeId:guid}")]
    public async Task<IActionResult> GetByStoreId(Guid storeId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDepartmentsByStoreIdQuery(storeId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new department</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing department</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in body.");

        var result = await _mediator.Send(command, cancellationToken);
        if (result is null) return NotFound($"Department with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Delete a department</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteDepartmentCommand(id), cancellationToken);
        if (!deleted) return NotFound($"Department with ID {id} was not found.");
        return NoContent();
    }
}
